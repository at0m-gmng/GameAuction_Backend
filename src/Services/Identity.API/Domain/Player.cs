using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Identity.API.Domain.ValueObjects;

namespace GameBackend.Services.Identity.API.Domain;

/// <summary>
/// Агрегат, представляющий игрока в системе.
/// Является фундаментом для аутентификации, баланса и инвентаря.
/// Хранит хеш пароля, а не сам пароль, для безопасности.
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
    /// Нормализованный email (lowercase) для поиска и уникальности.
    /// Позволяет aBc@gmail.com и ABC@gmail.com быть одним игроком.
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

    // TODO: неиспользуемое поле — реальный инвентарь теперь в Catalog.API (Item + InventoryItem).
    private readonly List<Guid> _inventory = new();

    /// <summary>
    /// IReadOnly-коллекция инвентаря для внешнего доступа.
    /// </summary>
    public IReadOnlyCollection<Guid> Inventory => _inventory.AsReadOnly();

    /// <summary>
    /// Признак того, что игроку уже выдан приветственный подарочный предмет.
    /// </summary>
    public bool WelcomeGiftGranted { get; private set; }

    /// <summary>
    /// Инициализирует нового игрока.
    /// </summary>
    /// <param name="nickname">Отображаемое имя.</param>
    /// <param name="email">Email в оригинальном написании.</param>
    /// <param name="passwordHash">Хеш пароля.</param>
    private Player(string nickname, string email, string passwordHash)
        : base(Guid.NewGuid())
    {
        Nickname = nickname;
        Email = email;
        NormalizedEmail = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Balance = GoldCredits.Zero;
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
    /// <returns>Новый экземпляр игрока.</returns>
    /// <exception cref="ArgumentException">Если любое из полей пустое.</exception>
    public static Player Register(string nickname, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Никнейм не может быть пустым", nameof(nickname));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Хеш пароля не может быть пустым", nameof(passwordHash));

        return new Player(nickname, email, passwordHash);
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
    /// Списывает средства с баланса (например, при покупке).
    /// Защищает правило: нельзя потратить больше, чем есть.
    /// </summary>
    /// <param name="amount">Сумма для списания.</param>
    /// <exception cref="InvalidOperationException">Если средств недостаточно.</exception>
    public void SpendFromBalance(GoldCredits amount)
    {
        Balance = Balance.Spend(amount);
    }

    /// <summary>
    /// Добавляет выигранный предмет в инвентарь игрока.
    /// </summary>
    /// <param name="itemId">Идентификатор предмета из каталога.</param>
    /// <exception cref="ArgumentException">Если itemId пустой.</exception>
    public void AddToInventory(Guid itemId)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException("Идентификатор предмета не может быть пустым", nameof(itemId));

        _inventory.Add(itemId);
    }

    /// <summary>
    /// Отмечает, что приветственный подарочный предмет выдан.
    /// </summary>
    public void MarkWelcomeGiftGranted()
    {
        WelcomeGiftGranted = true;
    }
}