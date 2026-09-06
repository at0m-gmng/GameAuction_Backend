using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Generation.API.Domain;

/// <summary>
/// Связанный юнит генерации: имя, описание, тип, превью и базовая цена идут вместе.
/// </summary>
public sealed class ItemArchetype : AggregateRoot
{
    /// <summary>
    /// Имя предмета.
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Описание предмета.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Категория предмета.
    /// </summary>
    public ItemCategory Category { get; private set; }

    /// <summary>
    /// URL превью предмета.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Базовая цена до применения множителя редкости.
    /// </summary>
    public decimal BasePrice { get; private set; }

    private ItemArchetype() { }

    private ItemArchetype(string name, string? description, ItemCategory category, string? imageUrl, decimal basePrice)
        : base(Guid.NewGuid())
    {
        Name = name; Description = description; Category = category;
        ImageUrl = imageUrl; BasePrice = basePrice;
    }

    /// <summary>
    /// Создаёт связанный юнит генерации.
    /// </summary>
    /// <param name="name">Имя предмета.</param>
    /// <param name="category">Категория предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="imageUrl">URL превью.</param>
    /// <param name="basePrice">Базовая цена.</param>
    public static ItemArchetype Create(string name, ItemCategory category, string? description = null, string? imageUrl = null, decimal basePrice = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым", nameof(name));
        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice));
        return new ItemArchetype(name, description, category, imageUrl, basePrice);
    }
}