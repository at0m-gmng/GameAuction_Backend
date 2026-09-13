using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных каталога: создаёт схему и наполняет публичную витрину.
/// </summary>
public static class CatalogDbInitializer
{
    /// <summary>
    /// Создаёт схему БД и держит витрину на уровне PublicCatalogSeedCount публичных лотов.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="generatePublicItem">Обработчик генерации одного публичного предмета.</param>
    /// <param name="marketplace">Параметры генерации витрины.</param>
    /// <param name="logger">Логгер для best-effort генерации, которая не должна ронять запуск сервиса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task SeedAsync(
        CatalogDbContext context,
        GeneratePublicItemCommandHandler generatePublicItem,
        MarketplaceSettings marketplace,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // NOTE: EnsureCreated вместо миграций — упрощение для демо-проекта.
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
            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Items" ADD COLUMN IF NOT EXISTS "IsListed" boolean NOT NULL DEFAULT false;""",
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """CREATE INDEX IF NOT EXISTS "IX_Items_IsListed" ON "Items" ("IsListed");""",
                cancellationToken);

            // NOTE: бэкфилл — публичные предметы, созданные до появления IsListed, иначе пропадают из витрины.
            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "Items" SET "IsListed" = true WHERE "OwnerId" IS NULL AND NOT "IsListed";""",
                cancellationToken);

            // NOTE: распроданный публичный лот (Stock=0) — фантом в витрине, снимаем его из листинга.
            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "Items" SET "IsListed" = false WHERE "OwnerId" IS NULL AND "IsListed" AND "Stock" <= 0;""",
                cancellationToken);

            // NOTE: чистим сирот инвентаря (карточка удалена) — иначе FK ниже не навесится на живую таблицу.
            await context.Database.ExecuteSqlRawAsync(
                """DELETE FROM "InventoryItems" WHERE "ItemId" NOT IN (SELECT "Id" FROM "Items");""",
                cancellationToken);

            // NOTE: FK с Restrict — навешиваем на существующую таблицу вручную, EnsureCreated его не доливает.
            await context.Database.ExecuteSqlRawAsync(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_InventoryItems_Items_ItemId'
                    ) THEN
                        ALTER TABLE "InventoryItems"
                            ADD CONSTRAINT "FK_InventoryItems_Items_ItemId"
                            FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE RESTRICT;
                    END IF;
                END $$;
                """,
                cancellationToken);
        }

        // NOTE: витрина держит ровно PublicCatalogSeedCount публичных лотов — лишнее снимаем, нехватку добираем.
        var listedPublicItems = await context.Items
            .Where(x => x.OwnerId == null && x.IsListed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (listedPublicItems.Count > marketplace.PublicCatalogSeedCount)
        {
            foreach (var surplus in listedPublicItems.Skip(marketplace.PublicCatalogSeedCount))
                surplus.Unlist();

            await context.SaveChangesAsync(cancellationToken);
        }

        var deficit = Math.Max(0, marketplace.PublicCatalogSeedCount - listedPublicItems.Count);

        for (var i = 0; i < deficit; i++)
        {
            try
            {
                await generatePublicItem.Handle(new GeneratePublicItemCommand(), cancellationToken);
            }
            catch (Exception ex)
            {
                // NOTE: генерация best-effort — недоступный Generation.API не должен ронять старт каталога.
                logger.LogWarning(ex, "Не удалось сгенерировать публичный предмет ({Done}/{Target})", i, deficit);
                break;
            }
        }
    }
}
