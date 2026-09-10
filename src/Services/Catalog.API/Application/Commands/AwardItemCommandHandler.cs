using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик выдачи предмета победителю аукциона: кладёт предмет в инвентарь, склад каталога не трогает.
/// </summary>
public sealed class AwardItemCommandHandler : ICommandHandler<AwardItemCommand, Guid?>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    public AwardItemCommandHandler(IItemRepository itemRepository, IInventoryRepository inventoryRepository)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
    }

    /// <summary>
    /// Начисляет победителю предмет в инвентарь и переписывает цену каталога на цену продажи; склад не трогает.
    /// Возвращает OwnerId предмета (продавца), если предмет был выставлен игроком, иначе null.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>OwnerId предмета (продавца) или null для публичного предмета.</returns>
    public async Task<Guid?> Handle(AwardItemCommand command, CancellationToken cancellationToken)
    {
        var quantity = command.Quantity < 1 ? 1 : command.Quantity;

        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
            ?? throw new InvalidOperationException("Предмет не найден");

        var sellerId = item.OwnerId;

        item.UpdatePrice(command.Price);
        item.Unlist();
        await _itemRepository.SaveAsync(item, cancellationToken);

        var inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(
            command.PlayerId,
            command.ItemId,
            cancellationToken);

        if (inventoryItem is null)
        {
            inventoryItem = InventoryItem.Create(command.PlayerId, command.ItemId);
            if (quantity > 1)
                inventoryItem.AddQuantity(quantity - 1);
        }
        else
        {
            inventoryItem.AddQuantity(quantity);
        }

        await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);

        return sellerId;
    }
}
