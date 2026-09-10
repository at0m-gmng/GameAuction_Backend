using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик снятия предмета с продажи — возвращает в инвентарь, если аукцион не начался.
/// </summary>
public sealed class UnlistItemCommandHandler : ICommandHandler<UnlistItemCommand>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    public UnlistItemCommandHandler(IItemRepository itemRepository, IInventoryRepository inventoryRepository)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
    }

    /// <summary>
    /// Снимает предмет с продажи и возвращает его в инвентарь владельца.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(UnlistItemCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException($"Предмет {command.ItemId} не найден");

        if (!item.IsListed)
            throw new InvalidOperationException("Предмет не выставлен на продажу");

        if (item.OwnerId != command.PlayerId)
            throw new InvalidOperationException("Вы не владелец этого предмета");

        item.Unlist();
        await _itemRepository.SaveAsync(item, cancellationToken);

        var inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(command.PlayerId, command.ItemId, cancellationToken);

        if (inventoryItem is null)
        {
            inventoryItem = InventoryItem.Create(command.PlayerId, command.ItemId);
        }
        else
        {
            inventoryItem.AddQuantity(1);
        }

        await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);
    }
}
