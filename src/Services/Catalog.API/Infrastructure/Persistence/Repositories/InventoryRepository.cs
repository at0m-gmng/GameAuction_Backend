using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория инвентаря на EF Core и PostgreSQL.
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий контекстом БД.
    /// </summary>
    /// <param name="context">Контекст базы данных каталога.</param>
    public InventoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> GetByPlayerAndItemAsync(Guid playerId, Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.ItemId == itemId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<InventoryItem>> GetByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Where(x => x.PlayerId == playerId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        var exists = await _context.InventoryItems.AnyAsync(x => x.Id == inventoryItem.Id, cancellationToken);

        if (exists)
            _context.InventoryItems.Update(inventoryItem);
        else
            _context.InventoryItems.Add(inventoryItem);

        await _context.SaveChangesAsync(cancellationToken);
    }
}