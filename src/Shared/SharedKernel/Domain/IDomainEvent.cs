namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Интерфейс для всех доменных событий в системе.
/// Доменное событие - это факт, который уже произошёл в бизнесе.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Уникальный идентификатор события.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Дата и время, когда событие произошло.
    /// </summary>
    DateTime OccurredOn { get; }
}