using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Generation.API.Domain;

/// <summary>
/// Уровень редкости с множителем цены. Итоговая цена = BasePrice × PriceMultiplier.
/// </summary>
public sealed class RarityTier : AggregateRoot
{
    /// <summary>
    /// Редкость.
    /// </summary>
    public ItemRarity Rarity { get; private set; }

    /// <summary>
    /// Множитель базовой цены.
    /// </summary>
    public decimal PriceMultiplier { get; private set; }

    private RarityTier() { }

    private RarityTier(ItemRarity rarity, decimal priceMultiplier) : base(Guid.NewGuid())
    {
        Rarity = rarity; PriceMultiplier = priceMultiplier;
    }

    /// <summary>
    /// Создаёт уровень редкости.
    /// </summary>
    /// <param name="rarity">Редкость.</param>
    /// <param name="priceMultiplier">Множитель базовой цены.</param>
    public static RarityTier Create(ItemRarity rarity, decimal priceMultiplier)
    {
        if (priceMultiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(priceMultiplier));
        return new RarityTier(rarity, priceMultiplier);
    }
}