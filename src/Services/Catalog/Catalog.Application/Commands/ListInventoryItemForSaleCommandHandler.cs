using GameBackend.Services.Catalog.Application.Interfaces;
using GameBackend.Services.Catalog.Domain;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Catalog.Application.Commands;

/// <summary>
/// Обработчик команды выставления предмета из инвентаря на продажу.
/// </summary>
public sealed class ListInventoryItemForSaleCommandHandler : ICommandHandler<ListInventoryItemForSaleCommand>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="unitOfWork">Единица работы (транзакция).</param>
    public ListInventoryItemForSaleCommandHandler(
        IInventoryRepository inventoryRepository,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (item.OwnerId is null)
            {
                // NOTE: публичная карточка общая — нельзя метить её лотом; создаём личную копию продавца.
                var lot = Item.CreateOwned(
                    command.PlayerId, item.Name, item.Description, item.Category, item.Rarity, item.ImageUrl, command.StartingPrice);
                lot.ListForSale(command.StartingPrice);
                await _itemRepository.SaveAsync(lot, token);
            }
            else
            {
                item.ListForSale(command.StartingPrice);
                await _itemRepository.SaveAsync(item, token);
            }

            inventoryItem.RemoveQuantity(1);

            if (inventoryItem.Quantity == 0)
                await _inventoryRepository.DeleteAsync(inventoryItem, token);
            else
                await _inventoryRepository.SaveAsync(inventoryItem, token);
        }, cancellationToken);
    }
}
