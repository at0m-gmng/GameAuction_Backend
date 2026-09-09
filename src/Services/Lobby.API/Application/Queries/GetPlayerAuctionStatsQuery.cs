using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Lobbies;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Запрос статистики побед и поражений игрока.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record GetPlayerAuctionStatsQuery(Guid PlayerId) : IQuery<PlayerAuctionStatsDto>;
