namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Редкость предмета (smallint, шаг 100), влияет на ценность и фильтр Rare+ — значения не менять, только добавлять.
/// </summary>
public enum ItemRarity : short
{
    /// <summary>Обычный предмет.</summary>
    Common = 100,

    /// <summary>Редкий предмет.</summary>
    Rare = 200,

    /// <summary>Эпический предмет.</summary>
    Epic = 300,

    /// <summary>Легендарный предмет.</summary>
    Legendary = 400
}