using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик команды выставления предмета из инвентаря на продажу.
/// </summary>
public sealed class ListInventoryItemForSaleCommandHandler : ICommandHandler<ListInventoryItemForSaleCommand>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="context">Контекст БД каталога (для транзакции).</param>
    public ListInventoryItemForSaleCommandHandler(
        IInventoryRepository inventoryRepository,
        IItemRepository itemRepository,
        CatalogDbContext context)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
        _context = context;
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

        // NOTE: CreateExecutionStrategy обязателен — EnableRetryOnFailure запрещает голый BeginTransactionAsync.
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            if (item.OwnerId is null)
            {
                // NOTE: публичная карточка общая — нельзя метить её лотом; создаём личную копию продавца.
                var lot = Item.CreateOwned(
                    command.PlayerId, item.Name, item.Description, item.Category, item.Rarity, item.ImageUrl, command.StartingPrice);
                lot.ListForSale(command.StartingPrice);
                await _itemRepository.SaveAsync(lot, cancellationToken);
            }
            else
            {
                item.ListForSale(command.StartingPrice);
                await _itemRepository.SaveAsync(item, cancellationToken);
            }

            inventoryItem.RemoveQuantity(1);

            if (inventoryItem.Quantity == 0)
                await _inventoryRepository.DeleteAsync(inventoryItem, cancellationToken);
            else
                await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
