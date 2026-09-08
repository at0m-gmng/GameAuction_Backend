using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain.Events;

/// <summary>
/// Событие: раунд торгов истёк без единой ставки, лобби сброшено обратно в Gathering.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
public sealed record RoundExpiredWithoutBids(Guid LobbyId) : IDomainEvent
{
    /// <summary>Уникальный идентификатор события.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Дата и время события.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
