namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Базовый класс объектов-значений — без идентификатора, определяются атрибутами, должны быть неизменяемыми.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Возвращает компоненты для сравнения объектов-значений — реализуется в классах-наследниках.
    /// </summary>
    /// <returns>Перечисление компонентов для сравнения.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Определяет равенство объектов-значений — сравнение по значениям компонентов, не по ссылкам.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>true, если значения компонентов равны; иначе false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Возвращает хеш-код на основе значений компонентов.
    /// </summary>
    /// <returns>Хеш-код объекта-значения.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Оператор сравнения двух объектов-значений на равенство.
    /// </summary>
    /// <param name="left">Первый объект.</param>
    /// <param name="right">Второй объект.</param>
    /// <returns>true, если значения равны; иначе false.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Оператор сравнения двух объектов-значений на неравенство.
    /// </summary>
    /// <param name="left">Первый объект.</param>
    /// <param name="right">Второй объект.</param>
    /// <returns>true, если значения не равны; иначе false.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}