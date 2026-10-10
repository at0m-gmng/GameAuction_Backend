using GameBackend.Services.Catalog.Application.Interfaces;
using GameBackend.Services.Catalog.Domain;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Catalog.Application.Commands;

/// <summary>
/// Обработчик выдачи предмета победителю аукциона: кладёт предмет в инвентарь, склад каталога не трогает.
/// </summary>
public sealed class AwardItemCommandHandler : ICommandHandler<AwardItemCommand, Guid?>
{
    private const int MinQuantity = 1;

    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProcessedOperationStore _processedOperations;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="processedOperations">Журнал идемпотентности.</param>
    /// <param name="unitOfWork">Единица работы (транзакция).</param>
    public AwardItemCommandHandler(
        IItemRepository itemRepository,
        IInventoryRepository inventoryRepository,
        IProcessedOperationStore processedOperations,
        IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
        _processedOperations = processedOperations;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Передаёт предмет победителю; повтор по тому же ключу возвращает сохранённого продавца без повторной выдачи.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>OwnerId продавца или null для публичного предмета.</returns>
    public async Task<Guid?> Handle(AwardItemCommand command, CancellationToken cancellationToken)
    {
        int quantity = Math.Max(command.Quantity, MinQuantity);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                Guid? sellerId;
                ProcessedOperationRecord? processed = await _processedOperations.FindAsync(command.IdempotencyKey, token);

                if (processed is not null)
                {
                    sellerId = Guid.TryParse(processed.Result, out Guid storedSeller) ? storedSeller : null;
                }
                else
                {
                    Item item = await _itemRepository.GetByIdAsync(command.ItemId, token)
                                ?? throw new InvalidOperationException("Предмет не найден");

                    sellerId = item.OwnerId;

                    item.UpdatePrice(command.Price);
                    item.Unlist();

                    if (sellerId.HasValue)
                    {
                        item.TransferTo(command.PlayerId);
                    }

                    await _itemRepository.SaveAsync(item, token);

                    InventoryItem? inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(command.PlayerId, command.ItemId, token);

                    if (inventoryItem is null)
                    {
                        inventoryItem = InventoryItem.Create(command.PlayerId, command.ItemId);
                        if (quantity > MinQuantity)
                        {
                            inventoryItem.AddQuantity(quantity - MinQuantity);
                        }
                    }
                    else
                    {
                        inventoryItem.AddQuantity(quantity);
                    }

                    _processedOperations.Add(command.IdempotencyKey, sellerId?.ToString());
                    await _inventoryRepository.SaveAsync(inventoryItem, token);
                }

                return sellerId;
            }, cancellationToken);
        }
        catch (DuplicateEntryException)
        {
            return null;
        }
    }
}
