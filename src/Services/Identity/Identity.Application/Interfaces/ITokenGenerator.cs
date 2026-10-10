namespace GameBackend.Services.Identity.Application.Interfaces;

/// <summary>
/// Выпуск токенов доступа игрока.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Выпускает токен доступа для игрока.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <returns>Подписанный токен.</returns>
    string Generate(Guid playerId);
}
