using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Catalog.API.Domain.Events;

namespace GameBackend.Services.Catalog.API.Domain;

/// <summary>
/// Агрегат, представляющий каталожную карточку предмета — шаблона для аукционных лотов.
/// Сам по себе не является аукционом. Аукцион создаётся в лобби на основе этой карточки.
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
    /// Начальная цена для аукциона (минимальная ставка).
    /// </summary>
    public decimal StartingPrice { get; private set; }

    /// <summary>
    /// Количество предметов данного типа в системе.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Инициализирует новую каталожную карточку предмета.
    /// </summary>
    /// <param name="name">Название предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="startingPrice">Начальная цена (не может быть отрицательной).</param>
    /// <param name="stock">Начальный остаток (не может быть отрицательным).</param>
    private Item(string name, string? description, decimal startingPrice, int stock)
        : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        StartingPrice = startingPrice;
        Stock = stock;

        AddDomainEvent(new ItemCreated(Id, Name, StartingPrice));
    }

    /// <summary>
    /// Приватный конструктор для поддержки ORM (Entity Framework Core).
    /// </summary>
    private Item()
    {
    }

    /// <summary>
    /// Фабричный метод для создания новой каталожной карточки предмета.
    /// Проверяет бизнес-правила перед созданием.
    /// </summary>
    /// <param name="name">Название предмета.</param>
    /// <param name="description">Описание предмета.</param>
    /// <param name="startingPrice">Начальная цена предмета.</param>
    /// <param name="stock">Начальный остаток.</param>
    /// <returns>Новый экземпляр каталожной карточки предмета.</returns>
    /// <exception cref="ArgumentException">Если название пустое.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Если цена или остаток отрицательные.</exception>
    public static Item Create(string name, string? description, decimal startingPrice, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название предмета не может быть пустым", nameof(name));

        if (startingPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(startingPrice), "Начальная цена не может быть отрицательной");

        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Остаток не может быть отрицательным");

        return new Item(name, description, startingPrice, stock);
    }

    /// <summary>
    /// Уменьшает остаток предметов на указанное количество.
    /// Защищает бизнес-правило: остаток не может быть меньше нуля.
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
}