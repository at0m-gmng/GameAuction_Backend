using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с лобби.
/// </summary>
public interface ILobbyRepository
{
    /// <summary>
    /// Получает лобби по идентификатору вместе со ставками и участниками.
    /// </summary>
    /// <param name="id">Идентификатор лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Лобби или null.</returns>
    Task<Lobby?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает список открытых лобби (статус Gathering) для экрана "Список лобби".
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