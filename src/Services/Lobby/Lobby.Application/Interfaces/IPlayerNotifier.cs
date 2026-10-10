namespace GameBackend.Services.Lobby.Application.Interfaces;

/// <summary>
/// Адресные realtime-уведомления игрокам.
/// </summary>
public interface IPlayerNotifier
{
    /// <summary>
    /// Сообщает игроку, что его баланс изменился и его нужно перечитать.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task NotifyBalanceChangedAsync(Guid playerId, CancellationToken cancellationToken);
}
