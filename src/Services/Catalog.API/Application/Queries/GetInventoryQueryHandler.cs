using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Application.Items;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Обработчик запроса инвентаря игрока.
/// </summary>
public sealed class GetInventoryQueryHandler : IQueryHandler<GetInventoryQuery, IReadOnlyCollection<InventoryItemDto>>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;

    /// <summary>
    /// Инициализирует обработчик репозиториями.
    /// </summary>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    public GetInventoryQueryHandler(IInventoryRepository inventoryRepository, IItemRepository itemRepository)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
    }
    public async Task<IReadOnlyCollection<InventoryItemDto>> Handle(
        GetInventoryQuery query,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByPlayerAsync(query.PlayerId, cancellationToken);

        if (inventory.Count == 0)
            return Array.Empty<InventoryItemDto>();

        var itemIds = inventory.Select(x => x.ItemId).ToList();
        var items = await _itemRepository.GetByIdsAsync(itemIds, cancellationToken);
        var itemsById = items.ToDictionary(x => x.Id);

        return inventory
            .Where(x => itemsById.ContainsKey(x.ItemId))
            .Select(x =>
            {
                var item = itemsById[x.ItemId];
                return new InventoryItemDto(
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Category,
                    item.Rarity,
                    item.ImageUrl,
                    x.Quantity,
                    x.AcquiredAt);
            })
            .ToArray();
    }
}