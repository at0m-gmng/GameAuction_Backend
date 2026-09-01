using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Domain.Events;

/// <summary>
/// Доменное событие, которое происходит при создании новой каталожной карточки предмета.
/// </summary>
/// <param name="ItemId">Уникальный идентификатор созданного предмета.</param>
/// <param name="Name">Название созданного предмета.</param>
/// <param name="StartingPrice">Начальная цена предмета.</param>
public sealed record ItemCreated(
    Guid ItemId,
    string Name,
    decimal StartingPrice
) : IDomainEvent
{
    /// <summary>
    /// Уникальный идентификатор события.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Дата и время, когда событие произошло.
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}