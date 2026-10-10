using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.Application.Lobbies;

namespace GameBackend.Services.Lobby.Application.Queries;

/// <summary>
/// Запрос детальной информации лобби для экрана лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
public sealed record GetLobbyQuery(Guid LobbyId) : IQuery<LobbyDetailsDto?>;