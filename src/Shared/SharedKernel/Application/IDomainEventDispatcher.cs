using GameBackend.SharedKernel.Domain;

namespace GameBackend.SharedKernel.Application;

/// <summary>
/// Контракт диспетчера доменных событий. Реализация решает, куда доставить события.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Доставляет набор доменных событий подписчикам.
    /// </summary>
    /// <param name="events">События для доставки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}