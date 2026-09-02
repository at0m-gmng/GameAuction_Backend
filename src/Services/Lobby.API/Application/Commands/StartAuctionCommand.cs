using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Команда: запустить аукцион в лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="Duration">Длительность аукциона.</param>
public sealed record StartAuctionCommand(Guid LobbyId, TimeSpan Duration) : ICommand;