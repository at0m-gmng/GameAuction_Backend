using GameBackend.Services.Generation.API.Application.Interfaces;
using GameBackend.Services.Generation.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Generation.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий пулов генерации на EF Core.
/// </summary>
public class GenerationPoolRepository : IGenerationPoolRepository
{
    private readonly GenerationDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий контекстом БД.
    /// </summary>
    /// <param name="context">Контекст БД генерации.</param>
    public GenerationPoolRepository(GenerationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Возвращает все архетипы предметов из БД генерации.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<ItemArchetype>> GetArchetypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ItemArchetypes.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает все уровни редкости из БД генерации.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<RarityTier>> GetRarityTiersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RarityTiers.ToListAsync(cancellationToken);
    }
}