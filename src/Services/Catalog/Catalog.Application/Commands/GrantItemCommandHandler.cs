using GameBackend.Services.Catalog.Application.Interfaces;
using GameBackend.Services.Catalog.Domain;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Catalog.Application.Commands;

/// <summary>
/// Обработчик выдачи приватного предмета: создаёт карточку и сразу отдаёт её игроку, атомарно.
/// </summary>
public sealed class GrantItemCommandHandler : ICommandHandler<GrantItemCommand, Guid>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProcessedOperationStore _processedOperations;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Инициализирует обработчик репозиториями, журналом идемпотентности и единицей работы.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="processedOperations">Журнал идемпотентности.</param>
    /// <param name="unitOfWork">Единица работы (транзакция).</param>
    public GrantItemCommandHandler(
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
    /// Создаёт приватную карточку и кладёт её в инвентарь; повтор по тому же ключу ничего не делает.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Id созданного предмета или <see cref="Guid.Empty"/>, если ключ уже обработан.</returns>
    public async Task<Guid> Handle(GrantItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                Guid itemId = Guid.Empty;

                if (await _processedOperations.FindAsync(command.IdempotencyKey, token) is null)
                {
                    Item item = Item.CreateOwned(
                        command.PlayerId,
                        command.Name,
                        command.Description,
                        command.Category,
                        command.Rarity,
                        command.ImageUrl,
                        command.StartingPrice);

                    await _itemRepository.SaveAsync(item, token);

                    _processedOperations.Add(command.IdempotencyKey);
                    await _inventoryRepository.SaveAsync(InventoryItem.Create(command.PlayerId, item.Id), token);
                    itemId = item.Id;
                }

                return itemId;
            }, cancellationToken);
        }
        catch (DuplicateEntryException)
        {
            return Guid.Empty;
        }
    }
}
