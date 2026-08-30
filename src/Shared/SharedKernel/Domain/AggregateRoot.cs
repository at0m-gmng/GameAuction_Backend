namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Базовый класс для всех агрегатов в системе.
/// Агрегат - это корень группы связанных объектов, которые должны изменяться вместе.
/// Агрегат гарантирует целостность данных внутри своей границы.
/// </summary>
public abstract class AggregateRoot : Entity
{
    /// <summary>
    /// Список доменных событий, которые произошли с этим агрегатом.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Коллекция доменных событий, ожидающих публикации.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Инициализирует новый агрегат с указанным идентификатором.
    /// </summary>
    /// <param name="id">Уникальный идентификатор агрегата.</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Инициализирует новый агрегат без идентификатора.
    /// Используется только для поддержки ORM (Entity Framework Core).
    /// </summary>
    protected AggregateRoot()
    {
        // Конструктор нужен для того, чтобы EF Core мог создавать объекты при чтении из БД
    }

    /// <summary>
    /// Добавляет доменное событие в список событий, ожидающих публикации.
    /// </summary>
    /// <param name="domainEvent">Доменное событие для добавления.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Очищает список доменных событий после их публикации.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}