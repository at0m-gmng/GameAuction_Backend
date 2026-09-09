using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса статистики побед и поражений игрока.
/// </summary>
public sealed class GetPlayerAuctionStatsQueryHandler : IQueryHandler<GetPlayerAuctionStatsQuery, PlayerAuctionStatsDto>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public GetPlayerAuctionStatsQueryHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает количество побед и поражений игрока.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<PlayerAuctionStatsDto> Handle(GetPlayerAuctionStatsQuery query, CancellationToken cancellationToken)
    {
        var (wins, losses) = await _repository.GetPlayerAuctionStatsAsync(query.PlayerId, cancellationToken);
        return new PlayerAuctionStatsDto(wins, losses);
    }
}
