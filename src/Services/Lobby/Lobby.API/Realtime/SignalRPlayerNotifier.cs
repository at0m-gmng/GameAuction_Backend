using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace GameBackend.Services.Lobby.API.Realtime;

/// <summary>
/// Адресные уведомления игрокам через SignalR-хаб лобби.
/// </summary>
public sealed class SignalRPlayerNotifier : IPlayerNotifier
{
    /// <summary>
    /// Имя клиентского метода «баланс изменился».
    /// </summary>
    public const string BalanceChangedMethod = "BalanceChanged";

    private readonly IHubContext<LobbyHub> _hub;

    /// <summary>
    /// Инициализирует уведомитель контекстом хаба.
    /// </summary>
    /// <param name="hub">Контекст SignalR-хаба лобби.</param>
    public SignalRPlayerNotifier(IHubContext<LobbyHub> hub) => _hub = hub;

    /// <inheritdoc />
    public Task NotifyBalanceChangedAsync(Guid playerId, CancellationToken cancellationToken) =>
        _hub.Clients.User(playerId.ToString()).SendAsync(BalanceChangedMethod, cancellationToken);
}
