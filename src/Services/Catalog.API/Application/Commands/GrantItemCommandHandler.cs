using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик выдачи приватного предмета: создаёт карточку предмета
/// и сразу добавляет её во владение игроку — атомарно, одной транзакцией.
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
        // NOTE: Item и InventoryItem сохраняются через два независимых репозитория,
        // каждый со своим SaveChangesAsync — без явной транзакции сбой на втором
        // шаге оставил бы висячую карточку предмета без владельца.
        //
        // CatalogDbContext настроен с EnableRetryOnFailure, поэтому вручную открыть
        // транзакцию через BeginTransactionAsync нельзя — EF Core это запрещает
        // (при ретрае пришлось бы повторять весь блок, а не отдельный запрос).
        // Обязательный паттерн для сочетания retry-стратегии с транзакцией —
        // выполнить её через CreateExecutionStrategy().
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

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

            await transaction.CommitAsync(cancellationToken);

            return item.Id;
        });
    }
}
