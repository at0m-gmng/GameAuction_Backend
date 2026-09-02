using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Команда: завершить аукцион и определить победителя.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
public sealed record CompleteAuctionCommand(Guid LobbyId) : ICommand;