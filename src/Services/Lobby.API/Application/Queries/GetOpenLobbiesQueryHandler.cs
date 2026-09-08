using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Application.Services;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса списка открытых лобби.
/// </summary>
public sealed class GetOpenLobbiesQueryHandler : IQueryHandler<GetOpenLobbiesQuery, IReadOnlyCollection<LobbyListDto>>
{
    private readonly ILobbyRepository _repository;
    private readonly AuctionCompletionService _auctionCompletion;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="auctionCompletion">Сервис завершения просроченных аукционов.</param>
    public GetOpenLobbiesQueryHandler(ILobbyRepository repository, AuctionCompletionService auctionCompletion)
    {
        _repository = repository;
        _auctionCompletion = auctionCompletion;
    }

    /// <summary>
    /// Возвращает список открытых лобби, лениво завершая просроченные (нет таймера — Render засыпает).
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<LobbyListDto>> Handle(GetOpenLobbiesQuery query, CancellationToken cancellationToken)
    {
        var lobbies = await _repository.GetOpenLobbiesAsync(cancellationToken);

        foreach (var lobby in lobbies)
            await _auctionCompletion.CompleteIfExpiredAsync(lobby, cancellationToken);

        return lobbies
            .Where(l => l.Status is LobbyStatus.Gathering or LobbyStatus.Bidding)
            .Select(ToListDto)
            .ToArray();
    }

    private static LobbyListDto ToListDto(LobbyAggregate lobby) => new(
        lobby.Id,
        lobby.ItemId,
        lobby.ItemName,
        lobby.ItemImageUrl,
        lobby.StartingPrice,
        lobby.Status,
        lobby.Participants.Count,
        lobby.MaxParticipants,
        lobby.CurrentBid?.Amount ?? lobby.StartingPrice,
        lobby.EndsAt);
}