namespace GameBackend.SharedKernel.Domain;

/// <summary>
/// Категория предмета (smallint, шаг 100) — никогда не менять присвоенные значения, только добавлять.
/// </summary>
public enum ItemCategory : short
{
    /// <summary>Оружие.</summary>
    Weapons = 100,

    /// <summary>Броня.</summary>
    Armor = 200,

    /// <summary>Техника и гаджеты.</summary>
    Tech = 300
}