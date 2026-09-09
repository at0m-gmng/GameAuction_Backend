using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория лобби.
/// </summary>
public interface ILobbyRepository
{
    /// <summary>
    /// Получает лобби по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Лобби или null.</returns>
    Task<LobbyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает открытые лобби (Gathering и Bidding) вместе со ставками.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция открытых лобби.</returns>
    Task<IReadOnlyCollection<LobbyAggregate>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает последние завершённые лобби (со ставками) для показа в общем списке аукционов.
    /// </summary>
    /// <param name="limit">Максимальное количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция завершённых лобби, новые первыми.</returns>
    Task<IReadOnlyCollection<LobbyAggregate>> GetRecentCompletedLobbiesAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает завершённые аукционы игрока, где он делал ставку (со ставками) — история для профиля.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция завершённых аукционов игрока, новые первыми.</returns>
    Task<IReadOnlyCollection<LobbyAggregate>> GetPlayerHistoryAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Считает завершённые аукционы игрока, где он делал ставку — победы и поражения.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество побед и поражений.</returns>
    Task<(int Wins, int Losses)> GetPlayerAuctionStatsAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет лобби.
    /// </summary>
    /// <param name="lobby">Лобби для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveAsync(LobbyAggregate lobby, CancellationToken cancellationToken = default);
}