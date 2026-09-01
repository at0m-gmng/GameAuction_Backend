using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Domain;
using GameBackend.Services.Lobby.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Lobby.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория лобби на EF Core + PostgreSQL.
/// </summary>
public class LobbyRepository : ILobbyRepository
{
    private readonly LobbyDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий с указанным контекстом.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public LobbyRepository(LobbyDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает лобби по идентификатору вместе со ставками.
    /// </summary>
    /// <param name="id">Идентификатор лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Лобби или null.</returns>
    public async Task<Lobby?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Lobbies
            .Include(x => x.Bids)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Получает открытые лобби (статус Gathering).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция открытых лобби.</returns>
    public async Task<IReadOnlyCollection<Lobby>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Lobbies
            .Where(x => x.Status == LobbyStatus.Gathering)
            .OrderBy(x => x.EndsAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Сохраняет лобби. Если оно ещё не отслеживается — добавляет.
    /// </summary>
    /// <param name="lobby">Лобби для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SaveAsync(Lobby lobby, CancellationToken cancellationToken = default)
    {
        // Если лобби уже загружено и отслеживается, EF сам применит изменения.
        // Добавляем только новые (detached) агрегаты.
        if (_context.Entry(lobby).State == EntityState.Detached)
            _context.Lobbies.Add(lobby);

        await _context.SaveChangesAsync(cancellationToken);
    }
}