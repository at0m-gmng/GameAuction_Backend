using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Domain;

/// <summary>
/// Позиция инвентаря игрока: сколько копий какого предмета куплено.
/// Одна строка на пару (игрок, предмет), количество растёт в Quantity.
/// </summary>
public sealed class InventoryItem : AggregateRoot
{
    /// <summary>Идентификатор игрока-владельца.</summary>
    public Guid PlayerId { get; private set; }

    /// <summary>Идентификатор предмета каталога.</summary>
    public Guid ItemId { get; private set; }

    /// <summary>Количество копий предмета.</summary>
    public int Quantity { get; private set; }

    /// <summary>Дата первой покупки (UTC).</summary>
    public DateTime AcquiredAt { get; private set; }

    /// <summary>Приватный конструктор для EF Core.</summary>
    private InventoryItem()
    {
    }

    private InventoryItem(Guid playerId, Guid itemId)
        : base(Guid.NewGuid())
    {
        PlayerId = playerId;
        ItemId = itemId;
        Quantity = 1;
        AcquiredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создаёт новую позицию инвентаря с одной копией предмета.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="itemId">Идентификатор предмета.</param>
    /// <returns>Новая позиция инвентаря.</returns>
    public static InventoryItem Create(Guid playerId, Guid itemId)
    {
        return new InventoryItem(playerId, itemId);
    }

    /// <summary>
    /// Добавляет копии предмета к существующей позиции.
    /// </summary>
    /// <param name="count">Сколько копий добавить.</param>
    public void AddQuantity(int count = 1)
    {
        Quantity += count;
    }
}