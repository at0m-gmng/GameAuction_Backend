using GameBackend.Services.Lobby.API.Domain.Events;
using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Domain;
using Microsoft.AspNetCore.SignalR;

namespace GameBackend.Services.Lobby.API.Infrastructure.Events;

/// <summary>
/// Диспетчер доменных событий через SignalR.
/// События конкретного лобби уходят в его группу, LobbyCreated — всем (обновить список).
/// </summary>
public sealed class SignalRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IHubContext<LobbyHub> _hub;

    /// <summary>
    /// Инициализирует диспетчер контекстом хаба.
    /// </summary>
    /// <param name="hub">Контекст SignalR-хаба лобби.</param>
    public SignalRDomainEventDispatcher(IHubContext<LobbyHub> hub)
    {
        _hub = hub;
    }

    /// <summary>
    /// Рассылает события по SignalR-группам.
    /// </summary>
    /// <param name="events">События для доставки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var e in events)
        {
            switch (e)
            {
                case LobbyCreated created:
                    await _hub.Clients.All.SendAsync("LobbyCreated", created, cancellationToken);
                    break;

                case PlayerJoinedLobby joined:
                    await _hub.Clients.Group(LobbyHub.GroupName(joined.LobbyId))
                        .SendAsync("PlayerJoinedLobby", joined, cancellationToken);
                    break;

                case BidPlaced bid:
                    await _hub.Clients.Group(LobbyHub.GroupName(bid.LobbyId))
                        .SendAsync("BidPlaced", bid, cancellationToken);
                    break;

                case AuctionCompleted completed:
                    await _hub.Clients.Group(LobbyHub.GroupName(completed.LobbyId))
                        .SendAsync("AuctionCompleted", completed, cancellationToken);
                    break;
            }
        }
    }
}