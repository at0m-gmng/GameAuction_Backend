using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.Application.Lobbies;

namespace GameBackend.Services.Lobby.Application.Queries;

/// <summary>
/// Запрос статистики побед и поражений игрока.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record GetPlayerAuctionStatsQuery(Guid PlayerId) : IQuery<PlayerAuctionStatsDto>;
