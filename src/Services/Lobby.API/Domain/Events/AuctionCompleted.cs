using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain.Events;

/// <summary>
/// Событие: аукцион завершён, определён победитель.
/// </summary>
/// <param name="LobbyId">Идентификатор лобби.</param>
/// <param name="WinnerId">Идентификатор победителя. Null, если ставок не было.</param>
/// <param name="FinalAmount">Финальная ставка. Null, если ставок не было.</param>
public sealed record AuctionCompleted(Guid LobbyId, Guid? WinnerId, decimal? FinalAmount) : IDomainEvent
{
    /// <summary>Уникальный идентификатор события.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Дата и время события.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}