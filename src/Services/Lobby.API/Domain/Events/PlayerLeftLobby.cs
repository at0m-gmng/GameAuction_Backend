using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain.Events;

/// <summary>
/// Событие: игрок покинул лобби.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="SlotsTaken">Сколько слотов занято после ухода.</param>
public sealed record PlayerLeftLobby(Guid LobbyId, Guid PlayerId, int SlotsTaken) : IDomainEvent
{
    /// <summary>Уникальный идентификатор события.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Дата и время события.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
