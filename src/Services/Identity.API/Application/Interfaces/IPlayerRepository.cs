using GameBackend.Services.Identity.API.Domain;

namespace GameBackend.Services.Identity.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория игроков.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Находит игрока по email.
    /// </summary>
    /// <param name="email">Email для поиска.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Игрок или null.</returns>
    Task<Player?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет нового игрока.
    /// </summary>
    /// <param name="player">Игрок для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveAsync(Player player, CancellationToken cancellationToken = default);
}