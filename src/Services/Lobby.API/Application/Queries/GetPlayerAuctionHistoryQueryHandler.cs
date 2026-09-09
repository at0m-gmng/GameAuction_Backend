using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса истории аукционов игрока.
/// </summary>
public sealed class GetPlayerAuctionHistoryQueryHandler
    : IQueryHandler<GetPlayerAuctionHistoryQuery, IReadOnlyCollection<PlayerAuctionHistoryDto>>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public GetPlayerAuctionHistoryQueryHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает завершённые аукционы игрока с пометкой победы и финальной ценой.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<PlayerAuctionHistoryDto>> Handle(
        GetPlayerAuctionHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var lobbies = await _repository.GetPlayerHistoryAsync(query.PlayerId, cancellationToken);

        return lobbies
            .Select(l => new PlayerAuctionHistoryDto(
                l.Id,
                l.ItemName,
                l.ItemImageUrl,
                l.ItemRarity,
                l.CurrentBid?.Amount ?? l.StartingPrice,
                l.WinnerId == query.PlayerId,
                l.EndsAt))
            .ToArray();
    }
}
