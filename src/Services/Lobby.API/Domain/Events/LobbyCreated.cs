using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain.Events;

/// <summary>
/// Событие: создано новое лобби для торгов за предмет.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="ItemId">Идентификатор предмета из каталога.</param>
/// <param name="MaxParticipants">Максимальное количество участников.</param>
public sealed record LobbyCreated(Guid LobbyId, Guid ItemId, int MaxParticipants) : IDomainEvent
{
    /// <summary>Уникальный идентификатор события.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Дата и время события.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}