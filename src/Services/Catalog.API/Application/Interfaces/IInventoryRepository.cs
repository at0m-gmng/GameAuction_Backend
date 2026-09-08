using GameBackend.Services.Catalog.API.Domain;

namespace GameBackend.Services.Catalog.API.Application.Interfaces;

/// <summary>
/// Контракт репозитория позиций инвентаря игроков.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Находит позицию инвентаря игрока по предмету.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="itemId">Идентификатор предмета.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Позиция инвентаря или null.</returns>
    Task<InventoryItem?> GetByPlayerAndItemAsync(Guid playerId, Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает все позиции инвентаря игрока.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция позиций инвентаря.</returns>
    Task<IReadOnlyCollection<InventoryItem>> GetByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет позицию инвентаря.
    /// </summary>
    /// <param name="inventoryItem">Позиция инвентаря.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет позицию инвентаря.
    /// </summary>
    /// <param name="inventoryItem">Позиция инвентаря для удаления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DeleteAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
}