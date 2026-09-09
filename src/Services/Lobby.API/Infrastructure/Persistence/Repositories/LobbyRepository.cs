using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Domain;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Lobby.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория лобби на EF Core + PostgreSQL — после сохранения рассылает доменные события.
/// </summary>
public class LobbyRepository : ILobbyRepository
{
    private readonly LobbyDbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;

    /// <summary>
    /// Инициализирует репозиторий контекстом и диспетчером событий.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="dispatcher">Диспетчер доменных событий.</param>
    public LobbyRepository(LobbyDbContext context, IDomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Получает лобби по идентификатору вместе со ставками.
    /// </summary>
    /// <param name="id">Идентификатор лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Лобби или null.</returns>
    public async Task<LobbyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Lobbies
            .Include(x => x.Bids)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Получает открытые лобби (Gathering и Bidding).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция открытых лобби.</returns>
    public async Task<IReadOnlyCollection<LobbyAggregate>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Lobbies
            .Where(x => x.Status == LobbyStatus.Gathering || x.Status == LobbyStatus.Bidding)
            .OrderBy(x => x.EndsAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Сохраняет лобби и рассылает накопленные доменные события.
    /// </summary>
    /// <param name="lobby">Лобби для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SaveAsync(LobbyAggregate lobby, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(lobby).State == EntityState.Detached)
        {
            _context.Lobbies.Add(lobby);
        }
        else
        {
            foreach (var bid in lobby.Bids)
            {
                var entry = _context.Entry(bid);
                Console.WriteLine($"[DIAG-BID] id={bid.Id} stateBefore={entry.State} isKeySet={entry.IsKeySet}");
                if (entry.State == EntityState.Detached)
                    entry.State = EntityState.Added;
                Console.WriteLine($"[DIAG-BID] id={bid.Id} stateAfter={entry.State}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var events = lobby.DomainEvents.ToArray();
        if (events.Length == 0)
            return;

        // NOTE: события рассылаются только после сохранения — иначе клиент узнает о том, что не заперсистилось.
        lobby.ClearDomainEvents();
        await _dispatcher.DispatchAsync(events, cancellationToken);
    }
}