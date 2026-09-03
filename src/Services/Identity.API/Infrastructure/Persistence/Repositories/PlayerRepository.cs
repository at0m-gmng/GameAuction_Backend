using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Identity.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория игроков на EF Core + PostgreSQL.
/// </summary>
public class PlayerRepository : IPlayerRepository
{
    private readonly IdentityDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий контекстом.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public PlayerRepository(IdentityDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Находит игрока по нормализованному email.
    /// </summary>
    /// <param name="normalizedEmail">Нормализованный email (lowercase).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Игрок или null.</returns>
    public async Task<Player?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Сохраняет нового игрока.
    /// </summary>
    /// <param name="player">Игрок для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SaveAsync(Player player, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(player).State == EntityState.Detached)
            _context.Players.Add(player);

        await _context.SaveChangesAsync(cancellationToken);
    }
}