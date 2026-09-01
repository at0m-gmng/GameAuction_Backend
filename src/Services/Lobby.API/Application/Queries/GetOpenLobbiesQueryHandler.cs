using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Queries;

/// <summary>
/// Обработчик запроса списка открытых лобби.
/// </summary>
public sealed class GetOpenLobbiesQueryHandler : IQueryHandler<GetOpenLobbiesQuery, IReadOnlyCollection<LobbyListDto>>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public GetOpenLobbiesQueryHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает список открытых лобби.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<LobbyListDto>> Handle(GetOpenLobbiesQuery query, CancellationToken cancellationToken)
    {
        var lobbies = await _repository.GetOpenLobbiesAsync(cancellationToken);
        return lobbies.Select(ToListDto).ToArray();
    }

    private static LobbyListDto ToListDto(Lobby lobby) => new(
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