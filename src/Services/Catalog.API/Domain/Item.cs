using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Catalog.API.Domain.Events;

namespace GameBackend.Services.Catalog.API.Domain;

/// <summary>
/// Агрегат каталожной карточки предмета — шаблона для аукционного лота, но не самого аукциона.
/// </summary>
public sealed class Item : AggregateRoot
{
    /// <summary>
    /// Название предмета.
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Описание предмета.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Категория предмета (оружие, броня, техника).
    /// </summary>
    public ItemCategory Category { get; private set; }

    /// <summary>
    /// Редкость предмета (обычный, редкий, эпический, легендарный).
    /// </summary>
    public ItemRarity Rarity { get; private set; }

    /// <summary>
    /// Ссылка на изображение предмета для витрины каталога.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Начальная цена для аукциона (минимальная ставка).
    /// </summary>
    public decimal StartingPrice { get; private set; }

    /// <summary>
    /// Количество предметов данного типа в системе.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Идентификатор игрока-владельца приватного предмета; null для публичной карточки каталога.
    /// </summary>
    public Guid? OwnerId { get; private set; }

    /// <summary>
    /// Предмет выставлен на продажу и виден в каталоге как лот для аукциона.
    /// </summary>
    public bool IsListed { get; private set; }

    /// <summary>
    /// Дата и время создания карточки (UTC) — по ней определяется, когда витрину пора пополнять.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Инициализирует новую каталожную карточку предмета.
    /// </summary>
    /// <param name="name">Название предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="category">Категория предмета.</param>
    /// <param name="rarity">Редкость предмета.</param>
    /// <param name="imageUrl">Ссылка на изображение.</param>
    /// <param name="startingPrice">Начальная цена (не может быть отрицательной).</param>
    /// <param name="stock">Начальный остаток (не может быть отрицательным).</param>
    /// <param name="ownerId">Владелец для приватного предмета, иначе null.</param>
    private Item(
        string name,
        string? description,
        ItemCategory category,
        ItemRarity rarity,
        string? imageUrl,
        decimal startingPrice,
        int stock,
        Guid? ownerId)
        : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        Category = category;
        Rarity = rarity;
        ImageUrl = imageUrl;
        StartingPrice = startingPrice;
        Stock = stock;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new ItemCreated(Id, Name, StartingPrice));
    }

    /// <summary>
    /// Приватный конструктор для поддержки ORM (Entity Framework Core).
    /// </summary>
    private Item()
    {
    }

    /// <summary>
    /// Фабричный метод для создания новой каталожной карточки предмета (с проверкой правил).
    /// </summary>
    /// <param name="name">Название предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="category">Категория предмета.</param>
    /// <param name="rarity">Редкость предмета.</param>
    /// <param name="imageUrl">Ссылка на изображение.</param>
    /// <param name="startingPrice">Начальная цена предмета.</param>
    /// <param name="stock">Начальный остаток.</param>
    /// <returns>Новый экземпляр каталожной карточки предмета.</returns>
    /// <exception cref="ArgumentException">Если название пустое.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Если цена или остаток отрицательные.</exception>
    public static Item Create(
        string name,
        string? description,
        ItemCategory category,
        ItemRarity rarity,
        string? imageUrl,
        decimal startingPrice,
        int stock)
    {
        ValidateCommonFields(name, startingPrice);

        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Остаток не может быть отрицательным");

        return new Item(name, description, category, rarity, imageUrl, startingPrice, stock, ownerId: null);
    }

    /// <summary>
    /// Фабричный метод для создания приватного предмета конкретного игрока (остаток всегда 0).
    /// </summary>
    /// <param name="ownerId">Идентификатор игрока-владельца.</param>
    /// <param name="name">Название предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="category">Категория предмета.</param>
    /// <param name="rarity">Редкость предмета.</param>
    /// <param name="imageUrl">Ссылка на изображение.</param>
    /// <param name="startingPrice">Начальная цена предмета.</param>
    /// <returns>Новый экземпляр приватной каталожной карточки предмета.</returns>
    /// <exception cref="ArgumentException">Если название пустое.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Если цена отрицательная.</exception>
    public static Item CreateOwned(
        Guid ownerId,
        string name,
        string? description,
        ItemCategory category,
        ItemRarity rarity,
        string? imageUrl,
        decimal startingPrice)
    {
        ValidateCommonFields(name, startingPrice);

        return new Item(name, description, category, rarity, imageUrl, startingPrice, stock: 0, ownerId);
    }

    private static void ValidateCommonFields(string name, decimal startingPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название предмета не может быть пустым", nameof(name));

        if (startingPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(startingPrice), "Начальная цена не может быть отрицательной");
    }

    /// <summary>
    /// Уменьшает остаток предметов; не даёт ему уйти в отрицательное значение.
    /// </summary>
    /// <param name="quantity">Количество для списания.</param>
    /// <exception cref="ArgumentOutOfRangeException">Если количество меньше или равно нулю.</exception>
    /// <exception cref="InvalidOperationException">Если остаток недостаточен.</exception>
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть больше нуля");

        if (Stock < quantity)
            throw new InvalidOperationException($"Недостаточный остаток. Доступно: {Stock}, запрошено: {quantity}");

        Stock -= quantity;
    }

    /// <summary>
    /// Увеличивает остаток предметов на указанное количество.
    /// </summary>
    /// <param name="quantity">Количество для добавления.</param>
    /// <exception cref="ArgumentOutOfRangeException">Если количество меньше или равно нулю.</exception>
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть больше нуля");

        Stock += quantity;
    }

    /// <summary>
    /// Устанавливает цену предмета по итогу продажи на аукционе.
    /// </summary>
    /// <param name="price">Новая цена (не может быть отрицательной).</param>
    /// <exception cref="ArgumentOutOfRangeException">Если цена отрицательная.</exception>
    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Цена не может быть отрицательной");

        StartingPrice = price;
    }

    /// <summary>
    /// Выставляет предмет на продажу по стартовой цене — он появляется в каталоге как лот.
    /// </summary>
    /// <param name="price">Стартовая цена (должна быть больше нуля).</param>
    /// <exception cref="ArgumentOutOfRangeException">Если цена не больше нуля.</exception>
    public void ListForSale(decimal price)
    {
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Цена продажи должна быть больше нуля");

        StartingPrice = price;
        IsListed = true;
    }

    /// <summary>
    /// Снимает предмет с продажи — он исчезает из каталога лотов.
    /// </summary>
    public void Unlist()
    {
        IsListed = false;
    }
}