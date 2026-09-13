using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик покупки предмета: спишет баланс, остаток в каталоге и добавит позицию в инвентарь.
/// </summary>
public sealed class BuyItemCommandHandler : ICommandHandler<BuyItemCommand>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IIdentityServiceClient _identityClient;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="identityClient">Клиент к Identity.API (списание баланса покупателя).</param>
    public BuyItemCommandHandler(
        IItemRepository itemRepository,
        IInventoryRepository inventoryRepository,
        IIdentityServiceClient identityClient)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
        _identityClient = identityClient;
    }

    public async Task Handle(BuyItemCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException("Предмет не найден");

        var totalPrice = item.StartingPrice * command.Quantity;

        item.DecreaseStock(command.Quantity);

        // NOTE: распродан — убираем из витрины, иначе фантомный лот с нулевым остатком остаётся видимым.
        if (item.Stock == 0)
            item.Unlist();

        // NOTE: списываем после проверки остатка, но до сохранения — неудачная оплата не должна давать предмет бесплатно.
        await _identityClient.DebitAsync(command.PlayerId, totalPrice, cancellationToken);

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