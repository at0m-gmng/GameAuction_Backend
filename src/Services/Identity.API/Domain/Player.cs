using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Identity.API.Domain.ValueObjects;

namespace GameBackend.Services.Identity.API.Domain;

/// <summary>
/// Агрегат игрока: аутентификация, баланс и инвентарь; хранит хеш пароля, а не сам пароль.
/// </summary>
public sealed class Player : AggregateRoot
{
    /// <summary>
    /// Отображаемое имя игрока.
    /// </summary>
    public string Nickname { get; private set; } = default!;

    /// <summary>
    /// Email игрока в оригинальном написании (для отображения).
    /// </summary>
    public string Email { get; private set; } = default!;

    /// <summary>
    /// Нормализованный email (lowercase) для поиска и уникальности — регистр не создаёт разных игроков.
    /// </summary>
    public string NormalizedEmail { get; private set; } = default!;

    /// <summary>
    /// Хеш пароля игрока. Сам пароль никогда не хранится.
    /// </summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>
    /// Баланс золотых кредитов игрока.
    /// </summary>
    public GoldCredits Balance { get; private set; } = GoldCredits.Zero;

    /// <summary>
    /// Дата и время регистрации игрока (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Признак того, что игроку уже выдан приветственный подарочный предмет.
    /// </summary>
    public bool WelcomeGiftGranted { get; private set; }

    /// <summary>
    /// Признак того, что игроку уже начислен стартовый баланс.
    /// </summary>
    public bool StartingBalanceGranted { get; private set; }

    /// <summary>
    /// Инициализирует нового игрока.
    /// </summary>
    /// <param name="nickname">Отображаемое имя.</param>
    /// <param name="email">Email в оригинальном написании.</param>
    /// <param name="passwordHash">Хеш пароля.</param>
    /// <param name="startingBalance">Баланс при регистрации.</param>
    private Player(string nickname, string email, string passwordHash, GoldCredits startingBalance)
        : base(Guid.NewGuid())
    {
        Nickname = nickname;
        Email = email;
        NormalizedEmail = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Balance = startingBalance;
        StartingBalanceGranted = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Приватный конструктор для поддержки ORM.
    /// </summary>
    private Player()
    {
    }

    /// <summary>
    /// Фабричный метод регистрации нового игрока.
    /// </summary>
    /// <param name="nickname">Отображаемое имя.</param>
    /// <param name="email">Email в оригинальном написании.</param>
    /// <param name="passwordHash">Хеш пароля (уже вычисленный).</param>
    /// <param name="startingBalance">Баланс, начисляемый при регистрации.</param>
    /// <returns>Новый экземпляр игрока.</returns>
    /// <exception cref="ArgumentException">Если любое из полей пустое.</exception>
    public static Player Register(string nickname, string email, string passwordHash, GoldCredits startingBalance)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Никнейм не может быть пустым", nameof(nickname));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Хеш пароля не может быть пустым", nameof(passwordHash));

        return new Player(nickname, email, passwordHash, startingBalance);
    }

    /// <summary>
    /// Пополняет баланс игрока (например, после победы в аукционе).
    /// </summary>
    /// <param name="amount">Сумма для пополнения.</param>
    public void AddToBalance(GoldCredits amount)
    {
        Balance = Balance.Add(amount);
    }

    /// <summary>
    /// Списывает средства с баланса (например, при покупке) — нельзя потратить больше, чем есть.
    /// </summary>
    /// <param name="amount">Сумма для списания.</param>
    /// <exception cref="InvalidOperationException">Если средств недостаточно.</exception>
    public void SpendFromBalance(GoldCredits amount)
    {
        Balance = Balance.Spend(amount);
    }

    /// <summary>
    /// Отмечает, что приветственный подарочный предмет выдан.
    /// </summary>
    public void MarkWelcomeGiftGranted()
    {
        WelcomeGiftGranted = true;
    }

    /// <summary>
    /// Начисляет стартовый баланс, если ещё не начислен — не переначисляет тем, кто уже получил его.
    /// </summary>
    /// <param name="startingBalance">Текущая настроенная сумма стартового баланса.</param>
    /// <returns>true, если начисление произошло.</returns>
    public bool GrantStartingBalanceIfNeeded(GoldCredits startingBalance)
    {
        if (StartingBalanceGranted)
            return false;

        Balance = Balance.Add(startingBalance);
        StartingBalanceGranted = true;
        return true;
    }
}