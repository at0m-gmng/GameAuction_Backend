using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса детальной информации лобби.
/// </summary>
public sealed class GetLobbyQueryHandler : IQueryHandler<GetLobbyQuery, LobbyDetailsDto?>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public GetLobbyQueryHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает детальную информацию лобби или null.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<LobbyDetailsDto?> Handle(GetLobbyQuery query, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(query.LobbyId, cancellationToken);
        if (lobby is null)
            return null;

        return new LobbyDetailsDto(
            lobby.Id,
            lobby.ItemId,
            lobby.ItemName,
            lobby.ItemImageUrl,
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