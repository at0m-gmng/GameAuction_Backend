using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.Application.Commands;

/// <summary>
/// Команда: сделать ставку в аукционе лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Amount">Сумма ставки.</param>
public sealed record PlaceBidCommand(Guid LobbyId, Guid PlayerId, decimal Amount) : ICommand;