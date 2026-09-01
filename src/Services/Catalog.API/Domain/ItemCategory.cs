namespace GameBackend.Services.Catalog.API.Domain;

/// <summary>
/// Категория предмета для фильтрации в каталоге.
/// </summary>
public enum ItemCategory
{
    /// <summary>Оружие.</summary>
    Weapons = 0,

    /// <summary>Броня.</summary>
    Armor = 1,

    /// <summary>Техника и гаджеты.</summary>
    Tech = 2
}