using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик покупки предмета.
/// Спишет остаток в каталоге и добавит позицию в инвентарь.
/// </summary>
public sealed class BuyItemCommandHandler : ICommandHandler<BuyItemCommand>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    public BuyItemCommandHandler(IItemRepository itemRepository, IInventoryRepository inventoryRepository)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task Handle(BuyItemCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException("Предмет не найден");

        item.DecreaseStock(command.Quantity);
        await _itemRepository.SaveAsync(item, cancellationToken);

        var inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(
            command.PlayerId,
            command.ItemId,
            cancellationToken);

        if (inventoryItem is null)
        {
            inventoryItem = InventoryItem.Create(command.PlayerId, command.ItemId);
            if (command.Quantity > 1)
                inventoryItem.AddQuantity(command.Quantity - 1);
        }
        else
        {
            inventoryItem.AddQuantity(command.Quantity);
        }

        await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);
    }
}