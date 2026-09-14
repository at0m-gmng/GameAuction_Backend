using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик выдачи предмета победителю аукциона: кладёт предмет в инвентарь, склад каталога не трогает.
/// </summary>
public sealed class AwardItemCommandHandler : ICommandHandler<AwardItemCommand, Guid?>
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="context">Контекст БД каталога (для транзакции и журнала идемпотентности).</param>
    public AwardItemCommandHandler(
        IItemRepository itemRepository,
        IInventoryRepository inventoryRepository,
        CatalogDbContext context)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
        _context = context;
    }

    /// <summary>
    /// Идемпотентно начисляет победителю предмет и переносит владельца; возвращает продавца (OwnerId) или null.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>OwnerId продавца или null для публичного предмета.</returns>
    public async Task<Guid?> Handle(AwardItemCommand command, CancellationToken cancellationToken)
    {
        var quantity = command.Quantity < 1 ? 1 : command.Quantity;

        // NOTE: CreateExecutionStrategy обязателен — EnableRetryOnFailure запрещает голый BeginTransactionAsync.
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // NOTE: ключ уже обработан — возвращаем сохранённого продавца, повторной выдачи нет.
            var processed = await _context.ProcessedOperations
                .FirstOrDefaultAsync(x => x.Key == command.IdempotencyKey, cancellationToken);
            if (processed is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return Guid.TryParse(processed.Result, out var storedSeller) ? storedSeller : (Guid?)null;
            }

            var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                ?? throw new InvalidOperationException("Предмет не найден");

            var sellerId = item.OwnerId;

            item.UpdatePrice(command.Price);
            item.Unlist();

            // NOTE: приватный лот переходит победителю — иначе перепродажа спишет/выплатит старому владельцу.
            if (sellerId.HasValue)
                item.TransferTo(command.PlayerId);

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

            _context.ProcessedOperations.Add(new ProcessedOperation(command.IdempotencyKey, sellerId?.ToString()));

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return sellerId;
            }
            catch (DbUpdateException)
            {
                // NOTE: гонка одновременных выдач — ключ уже вставлен другим запросом; транзакция откатится.
                return (Guid?)null;
            }
        });
    }
}
