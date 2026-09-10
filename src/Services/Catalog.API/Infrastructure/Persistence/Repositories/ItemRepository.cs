using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория предметов на основе EF Core и PostgreSQL.
/// </summary>
public class ItemRepository : IItemRepository
{
    /// <summary>
    /// Контекст базы данных каталога.
    /// </summary>
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий с указанным контекстом.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public ItemRepository(CatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает предмет по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор предмета.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Предмет или null.</returns>
    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Item>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Items.Where(x => idList.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Получает список предметов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="category">Категория или null.</param>
    /// <param name="minimumRarity">Минимальная редкость или null.</param>
    /// <param name="skip">Сколько пропустить.</param>
    /// <param name="take">Сколько взять.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция предметов.</returns>
    public async Task<IReadOnlyCollection<Item>> GetListAsync(
        ItemCategory? category,
        ItemRarity? minimumRarity,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        // NOTE: каталог — это витрина выставленных лотов; невыставленные (в т.ч. инвентарь игроков) сюда не попадают.
        var query = _context.Items.Where(x => x.IsListed);

        if (category.HasValue)
            query = query.Where(x => x.Category == category.Value);

        if (minimumRarity.HasValue)
            query = query.Where(x => x.Rarity >= minimumRarity.Value);

        return await query
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Сохраняет предмет (создаёт или обновляет).
    /// </summary>
    /// <param name="item">Предмет для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SaveAsync(Item item, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Items.AnyAsync(x => x.Id == item.Id, cancellationToken);

        if (exists)
            _context.Items.Update(item);
        else
            _context.Items.Add(item);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Удаляет предмет.
    /// </summary>
    /// <param name="item">Предмет для удаления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task DeleteAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}