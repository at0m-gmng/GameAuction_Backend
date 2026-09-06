using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных каталога: создаёт схему и наполняет стартовыми предметами.
/// </summary>
public static class CatalogDbInitializer
{
    /// <summary>
    /// Создаёт схему БД и, если каталог пуст, наполняет его стартовыми предметами.
    /// Идемпотентен: повторный вызов не дублирует данные.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        // Создаёт схему из модели, если БД ещё нет.
        // Для production предпочтительны миграции; здесь — простой путь для демо.
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // NOTE: EnsureCreated не доливает колонки в уже существующую БД. Патч
        // идемпотентен (IF NOT EXISTS) — безопасен и на новой, и на старой БД.
        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "Items" ADD COLUMN IF NOT EXISTS "OwnerId" uuid NULL;""",
            cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Items_OwnerId" ON "Items" ("OwnerId");""",
            cancellationToken);

        // Идемпотентность: если данные уже есть, не сеем повторно.
        if (await context.Items.AnyAsync(cancellationToken))
            return;

        var items = new[]
        {
            Item.Create("Kang Tao EMP Cannon", "Электромагнитная пушка корпоративного класса.", ItemCategory.Weapons, ItemRarity.Epic, null, 185000m, 1),
            Item.Create("Arasaka Smart Pistol", "Умный пистолет с самонаведением.", ItemCategory.Weapons, ItemRarity.Rare, null, 95000m, 2),
            Item.Create("Militech Assault Rifle", "Надёжная штурмовая винтовка.", ItemCategory.Weapons, ItemRarity.Common, null, 45000m, 5),
            Item.Create("Kang Tao Reactive Armor", "Реактивная броня с самовосстановлением.", ItemCategory.Armor, ItemRarity.Epic, null, 150000m, 1),
            Item.Create("Trauma Team Field Vest", "Полевой жилет медиков Trauma Team.", ItemCategory.Armor, ItemRarity.Rare, null, 80000m, 3),
            Item.Create("Netrunner Cyberdeck", "Кибердек для взлома корпоративных сетей.", ItemCategory.Tech, ItemRarity.Legendary, null, 300000m, 1),
            Item.Create("Kiroshi Optics", "Кибероптика с тактическим оверлеем.", ItemCategory.Tech, ItemRarity.Rare, null, 70000m, 4),
        };

        context.Items.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);
    }
}