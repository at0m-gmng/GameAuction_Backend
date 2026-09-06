using GameBackend.Services.Generation.API.Domain;
using GameBackend.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Generation.API.Infrastructure.Persistence;

/// <summary>
/// Создаёт схему и наполняет пулы стартовыми данными.
/// </summary>
public static class GenerationDbInitializer
{
    /// <summary>
    /// Создаёт таблицы и сеет архетипы и уровни редкости, если их ещё нет.
    /// </summary>
    /// <param name="db">Контекст БД генерации.</param>
    public static async Task SeedAsync(GenerationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.ItemArchetypes.AnyAsync())
            return;

        db.ItemArchetypes.AddRange(
            ItemArchetype.Create("Rusty Blade", ItemCategory.Weapons, "A worn but reliable blade", "https://cdn.example.com/blade.png", 100),
            ItemArchetype.Create("Plasma Blaster", ItemCategory.Weapons, "Still holds a charge", "https://cdn.example.com/blaster.png", 150),
            ItemArchetype.Create("Plasma Vest", ItemCategory.Armor, "Scorched but solid", "https://cdn.example.com/vest.png", 250),
            ItemArchetype.Create("Nano Visor", ItemCategory.Armor, "Sees through smoke", "https://cdn.example.com/visor.png", 200),
            ItemArchetype.Create("Nano Drone", ItemCategory.Tech, "Hums quietly", "https://cdn.example.com/drone.png", 300),
            ItemArchetype.Create("Salvaged Scanner", ItemCategory.Tech, "Finds what others miss", "https://cdn.example.com/scanner.png", 180));

        // Дубликат Common — выпадает чаще при равновероятном выборе.
        db.RarityTiers.AddRange(
            RarityTier.Create(ItemRarity.Common, 1),
            RarityTier.Create(ItemRarity.Common, 1),
            RarityTier.Create(ItemRarity.Rare, 2),
            RarityTier.Create(ItemRarity.Epic, 4),
            RarityTier.Create(ItemRarity.Legendary, 8));

        await db.SaveChangesAsync();
    }
}