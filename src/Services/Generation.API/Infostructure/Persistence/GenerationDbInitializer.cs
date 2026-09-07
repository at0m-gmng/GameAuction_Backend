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

        // NOTE: таблицы проверяются раздельно, чтобы чистка одной не задублировала другую.
        if (!await db.ItemArchetypes.AnyAsync())
        {
            // NOTE: картинки раздаются со статики Nexus Exchange (public/items), не отсюда.
            const string imageBaseUrl = "https://at0m-gmng.github.io/GameAuction_Front/items";

            db.ItemArchetypes.AddRange(
                ItemArchetype.Create("Rusty Blade", ItemCategory.Weapons, "A worn but reliable blade", $"{imageBaseUrl}/blade.png", 100),
                ItemArchetype.Create("Plasma Blaster", ItemCategory.Weapons, "Still holds a charge", $"{imageBaseUrl}/blaster.png", 150),
                ItemArchetype.Create("Plasma Vest", ItemCategory.Armor, "Scorched but solid", $"{imageBaseUrl}/vest.png", 250),
                ItemArchetype.Create("Nano Visor", ItemCategory.Armor, "Sees through smoke", $"{imageBaseUrl}/visor.png", 200),
                ItemArchetype.Create("Nano Drone", ItemCategory.Tech, "Hums quietly", $"{imageBaseUrl}/drone.png", 300),
                ItemArchetype.Create("Salvaged Scanner", ItemCategory.Tech, "Finds what others miss", $"{imageBaseUrl}/scanner.png", 180));
        }

        if (!await db.RarityTiers.AnyAsync())
        {
            // NOTE: Common указан дважды — выпадает чаще при равновероятном выборе.
            db.RarityTiers.AddRange(
                RarityTier.Create(ItemRarity.Common, 1),
                RarityTier.Create(ItemRarity.Common, 1),
                RarityTier.Create(ItemRarity.Rare, 2),
                RarityTier.Create(ItemRarity.Epic, 4),
                RarityTier.Create(ItemRarity.Legendary, 8));
        }

        await db.SaveChangesAsync();
    }
}