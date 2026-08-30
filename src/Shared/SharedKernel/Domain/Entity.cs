namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Базовый класс для всех сущностей в системе.
/// Сущность - это объект с уникальным идентификатором, 
/// который сохраняет свою идентичность независимо от изменений данных.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Уникальный идентификатор сущности.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Инициализирует новую сущность с указанным идентификатором.
    /// </summary>
    /// <param name="id">Уникальный идентификатор сущности.</param>
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Инициализирует новую сущность без идентификатора.
    /// Используется только для поддержки ORM (Entity Framework Core).
    /// </summary>
    protected Entity()
    {
        // Конструктор нужен для того, чтобы EF Core мог создавать объекты при чтении из БД
    }

    /// <summary>
    /// Определяет, равна ли текущая сущность другому объекту.
    /// Сущности считаются равными, если они одного типа и имеют одинаковый идентификатор.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>true, если сущности равны; иначе false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Возвращает хеш-код сущности на основе её идентификатора.
    /// </summary>
    /// <returns>Хеш-код сущности.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Оператор сравнения двух сущностей на равенство.
    /// </summary>
    /// <param name="left">Первая сущность.</param>
    /// <param name="right">Вторая сущность.</param>
    /// <returns>true, если сущности равны; иначе false.</returns>
    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Оператор сравнения двух сущностей на неравенство.
    /// </summary>
    /// <param name="left">Первая сущность.</param>
    /// <param name="right">Вторая сущность.</param>
    /// <returns>true, если сущности не равны; иначе false.</returns>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}