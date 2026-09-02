using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Команда: игрок присоединяется к лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record JoinLobbyCommand(Guid LobbyId, Guid PlayerId) : ICommand;