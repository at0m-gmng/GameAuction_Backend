using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Команда: игрок покидает лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record LeaveLobbyCommand(Guid LobbyId, Guid PlayerId) : ICommand;
