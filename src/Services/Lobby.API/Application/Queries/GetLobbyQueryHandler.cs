using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Application.Services;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса детальной информации лобби.
/// </summary>
public sealed class GetLobbyQueryHandler : IQueryHandler<GetLobbyQuery, LobbyDetailsDto?>
{
    private readonly ILobbyRepository _repository;
    private readonly AuctionCompletionService _auctionCompletion;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="auctionCompletion">Сервис завершения просроченных аукционов.</param>
    public GetLobbyQueryHandler(ILobbyRepository repository, AuctionCompletionService auctionCompletion)
    {
        _repository = repository;
        _auctionCompletion = auctionCompletion;
    }

    /// <summary>
    /// Возвращает детальную информацию лобби или null, попутно лениво завершая просроченный аукцион.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<LobbyDetailsDto?> Handle(GetLobbyQuery query, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(query.LobbyId, cancellationToken);
        if (lobby is null)
            return null;

        await _auctionCompletion.CompleteIfExpiredAsync(lobby, cancellationToken);

        return new LobbyDetailsDto(
            lobby.Id,
            lobby.ItemId,
            lobby.ItemName,
            lobby.ItemImageUrl,
            lobby.ItemRarity,
            lobby.StartingPrice,
            lobby.Status,
            lobby.MaxParticipants,
            lobby.Participants,
            lobby.CurrentBid?.Amount ?? lobby.StartingPrice,
            lobby.CurrentBid?.PlayerId,
            lobby.EndsAt,
            lobby.WinnerId,
            lobby.Bids.Select(b => new LobbyBidDto(b.PlayerId, b.Amount, b.PlacedAt)).ToArray());
    }
}