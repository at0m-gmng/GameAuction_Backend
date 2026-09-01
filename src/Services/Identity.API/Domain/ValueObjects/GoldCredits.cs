using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Identity.API.Domain.ValueObjects;

/// <summary>
/// Объект-значение, представляющий количество золотых кредитов игрока.
/// Неизменяемый: все операции возвращают новый экземпляр.
/// Защищает бизнес-правило: количество кредитов не может быть отрицательным.
/// </summary>
public sealed class GoldCredits : ValueObject
{
    /// <summary>
    /// Количество золотых кредитов.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Инициализирует объект-значение с указанным количеством кредитов.
    /// </summary>
    /// <param name="amount">Количество кредитов (не может быть отрицательным).</param>
    /// <exception cref="ArgumentOutOfRangeException">Если количество отрицательное.</exception>
    public GoldCredits(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Количество кредитов не может быть отрицательным");

        Amount = amount;
    }

    /// <summary>
    /// Нулевой баланс.
    /// </summary>
    public static GoldCredits Zero => new(0);

    /// <summary>
    /// Возвращает новый объект с увеличенным количеством кредитов.
    /// </summary>
    /// <param name="other">Сумма для добавления.</param>
    /// <returns>Новый экземпляр с увеличенным балансом.</returns>
    public GoldCredits Add(GoldCredits other)
    {
        return new GoldCredits(Amount + other.Amount);
    }

    /// <summary>
    /// Возвращает новый объект с уменьшенным количеством кредитов.
    /// Защищает правило: нельзя потратить больше, чем есть.
    /// </summary>
    /// <param name="other">Сумма для списания.</param>
    /// <returns>Новый экземпляр с уменьшенным балансом.</returns>
    /// <exception cref="InvalidOperationException">Если средств недостаточно.</exception>
    public GoldCredits Spend(GoldCredits other)
    {
        if (other.Amount > Amount)
            throw new InvalidOperationException($"Недостаточно средств. Доступно: {Amount}, требуется: {other.Amount}");

        return new GoldCredits(Amount - other.Amount);
    }

    /// <summary>
    /// Возвращает компоненты для сравнения объектов-значений.
    /// </summary>
    /// <returns>Компоненты для сравнения.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}