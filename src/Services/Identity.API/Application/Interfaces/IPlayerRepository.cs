using GameBackend.Services.Identity.API.Domain;

namespace GameBackend.Services.Identity.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория игроков.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Находит игрока по нормализованному email (lowercase).
    /// </summary>
    /// <param name="normalizedEmail">Нормализованный email.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Игрок или null.</returns>
    Task<Player?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Находит игрока по идентификатору.
    /// </summary>
    Task<Player?> GetAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет нового игрока.
    /// </summary>
    /// <param name="player">Игрок для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveAsync(Player player, CancellationToken cancellationToken = default);
}