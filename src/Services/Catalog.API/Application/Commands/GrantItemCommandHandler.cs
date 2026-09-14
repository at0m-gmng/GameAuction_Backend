using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик выдачи приватного предмета: создаёт карточку и сразу отдаёт её игроку, атомарно.
/// </summary>
public sealed class GrantItemCommandHandler : ICommandHandler<GrantItemCommand, Guid>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Инициализирует обработчик репозиториями и контекстом БД (для транзакции).
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="context">Контекст БД каталога.</param>
    public GrantItemCommandHandler(
        IItemRepository itemRepository,
        IInventoryRepository inventoryRepository,
        CatalogDbContext context)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
        _context = context;
    }

    public async Task<Guid> Handle(GrantItemCommand command, CancellationToken cancellationToken)
    {
        // NOTE: CreateExecutionStrategy обязателен — EnableRetryOnFailure запрещает голый BeginTransactionAsync.
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // NOTE: ключ уже обработан — повторная выдача пропускается (идемпотентность).
            if (await _context.ProcessedOperations.AnyAsync(x => x.Key == command.IdempotencyKey, cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return Guid.Empty;
            }

            var item = Item.CreateOwned(
                command.PlayerId,
                command.Name,
                command.Description,
                command.Category,
                command.Rarity,
                command.ImageUrl,
                command.StartingPrice);

            await _itemRepository.SaveAsync(item, cancellationToken);

            var inventoryItem = InventoryItem.Create(command.PlayerId, item.Id);
            await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);

            _context.ProcessedOperations.Add(new ProcessedOperation(command.IdempotencyKey));

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return item.Id;
            }
            catch (DbUpdateException)
            {
                // NOTE: гонка одновременных выдач — ключ уже вставлен другим запросом; транзакция откатится.
                return Guid.Empty;
            }
        });
    }
}
