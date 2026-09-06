namespace GameBackend.Services.Generation.API.Domain;

/// <summary>
/// Генератор заготовок: случайный архетип + случайный уровень редкости,
/// итоговая цена = BasePrice × PriceMultiplier. Без состояния.
/// </summary>
public sealed class ItemGenerator : IItemGenerator
{
    /// <summary>
    /// Выбирает случайный архетип и уровень редкости, считает итоговую цену.
    /// </summary>
    /// <param name="archetypes">Пул архетипов предметов.</param>
    /// <param name="tiers">Пул уровней редкости.</param>
    public GeneratedItemDto Create(IReadOnlyCollection<ItemArchetype> archetypes, IReadOnlyCollection<RarityTier> tiers)
    {
        if (archetypes.Count == 0 || tiers.Count == 0)
            throw new InvalidOperationException("Пулы генерации не заполнены");

        var random = Random.Shared;

        var archetype = archetypes.ElementAt(random.Next(archetypes.Count));
        var tier = tiers.ElementAt(random.Next(tiers.Count));

        var finalPrice = Math.Round(archetype.BasePrice * tier.PriceMultiplier, 2);

        return new GeneratedItemDto(
            archetype.Name,
            archetype.Description,
            archetype.Category,
            tier.Rarity,
            archetype.ImageUrl,
            finalPrice);
    }
}