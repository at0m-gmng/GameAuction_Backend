using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.Application.Lobbies;

namespace GameBackend.Services.Lobby.Application.Queries;

/// <summary>
/// Запрос списка открытых лобби для экрана "Список лобби".
/// </summary>
public sealed record GetOpenLobbiesQuery : IQuery<IReadOnlyCollection<LobbyListDto>>;