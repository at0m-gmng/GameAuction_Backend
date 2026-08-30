namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Базовый класс для объектов-значений (Value Objects).
/// Объект-значение не имеет уникального идентификатора и определяется только своими атрибутами.
/// Объекты-значения должны быть неизменяемыми (immutable).
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Возвращает компоненты (поля), которые используются для сравнения объектов-значений.
    /// Должен быть реализован в классах-наследниках.
    /// </summary>
    /// <returns>Перечисление компонентов для сравнения.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Определяет, равен ли текущий объект-значение другому объекту.
    /// Сравнение происходит по значениям компонентов, а не по ссылкам в памяти.
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