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
    Task<Lobby?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает открытые лобби (Gathering и Bidding).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция открытых лобби.</returns>
    Task<IReadOnlyCollection<Lobby>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет лобби.
    /// </summary>
    /// <param name="lobby">Лобби для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveAsync(Lobby lobby, CancellationToken cancellationToken = default);
}