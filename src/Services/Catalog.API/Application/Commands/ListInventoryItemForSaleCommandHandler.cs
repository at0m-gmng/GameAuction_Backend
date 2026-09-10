using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик команды выставления предмета из инвентаря на продажу.
/// </summary>
public sealed class ListInventoryItemForSaleCommandHandler : ICommandHandler<ListInventoryItemForSaleCommand>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    public ListInventoryItemForSaleCommandHandler(
        IInventoryRepository inventoryRepository,
        IItemRepository itemRepository)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Помечает предмет выставленным по цене и списывает его из инвентаря игрока; лобби не создаёт.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(ListInventoryItemForSaleCommand command, CancellationToken cancellationToken)
    {
        if (command.StartingPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.StartingPrice), "Стартовая цена должна быть больше нуля");

        var inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(command.PlayerId, command.ItemId, cancellationToken);
        if (inventoryItem is null || inventoryItem.Quantity < 1)
            throw new InvalidOperationException("Предмет не найден в инвентаре");

        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException($"Предмет {command.ItemId} не найден в каталоге");

        item.ListForSale(command.StartingPrice);
        await _itemRepository.SaveAsync(item, cancellationToken);

        inventoryItem.RemoveQuantity(1);

        if (inventoryItem.Quantity == 0)
            await _inventoryRepository.DeleteAsync(inventoryItem, cancellationToken);
        else
            await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);
    }
}
