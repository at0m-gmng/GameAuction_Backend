using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain.Events;

/// <summary>
/// Событие: игрок сделал ставку в аукционе.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Amount">Сумма ставки.</param>
public sealed record BidPlaced(Guid LobbyId, Guid PlayerId, decimal Amount) : IDomainEvent
{
    /// <summary>Уникальный идентификатор события.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Дата и время события.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}