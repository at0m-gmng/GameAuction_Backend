using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.Application.Lobbies;

namespace GameBackend.Services.Lobby.Application.Queries;

/// <summary>
/// Запрос истории завершённых аукционов игрока.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record GetPlayerAuctionHistoryQuery(Guid PlayerId) : IQuery<IReadOnlyCollection<PlayerAuctionHistoryDto>>;
