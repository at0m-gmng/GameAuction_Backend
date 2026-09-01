namespace GameBackend.Services.Catalog.API.Domain;

/// <summary>
/// Редкость предмета. Влияет на ценность и используется фильтром "Rare+".
/// </summary>
public enum ItemRarity
{
    /// <summary>Обычный предмет.</summary>
    Common = 0,

    /// <summary>Редкий предмет.</summary>
    Rare = 1,

    /// <summary>Эпический предмет.</summary>
    Epic = 2,

    /// <summary>Легендарный предмет.</summary>
    Legendary = 3
}