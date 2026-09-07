using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных каталога: создаёт схему и наполняет публичную витрину.
/// </summary>
public static class CatalogDbInitializer
{
    /// <summary>
    /// Создаёт схему БД и генерирует публичные предметы витрины через Generation.API —
    /// с нуля, если витрина пуста, или добавляет партию, если прошлое пополнение
    /// было больше <see cref="MarketplaceSettings.RestockIntervalHours"/> назад.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="generatePublicItem">Обработчик генерации одного публичного предмета.</param>
    /// <param name="marketplace">Параметры генерации витрины.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task SeedAsync(
        CatalogDbContext context,
        GeneratePublicItemCommandHandler generatePublicItem,
        MarketplaceSettings marketplace,
        CancellationToken cancellationToken = default)
    {
        // Создаёт схему из модели, если БД ещё нет.
        // Для production предпочтительны миграции; здесь — простой путь для демо.
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // NOTE: EnsureCreated не доливает колонки в старую БД — патч идемпотентен, только для Postgres.
        if (context.Database.IsNpgsql())
        {
            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Items" ADD COLUMN IF NOT EXISTS "OwnerId" uuid NULL;""",
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """CREATE INDEX IF NOT EXISTS "IX_Items_OwnerId" ON "Items" ("OwnerId");""",
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Items" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT '2000-01-01';""",
                cancellationToken);
        }

        var newestPublicItemCreatedAt = await context.Items
            .Where(x => x.OwnerId == null)
            .Select(x => (DateTime?)x.CreatedAt)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);

        int batchSize;
        if (newestPublicItemCreatedAt is null)
        {
            batchSize = marketplace.PublicCatalogSeedCount;
        }
        else if (newestPublicItemCreatedAt < DateTime.UtcNow.AddHours(-marketplace.RestockIntervalHours))
        {
            batchSize = marketplace.RestockBatchSize;
        }
        else
        {
            return;
        }

        for (var i = 0; i < batchSize; i++)
            await generatePublicItem.Handle(new GeneratePublicItemCommand(), cancellationToken);
    }
}
