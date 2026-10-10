namespace GameBackend.Services.Identity.Application.Configuration;

/// <summary>
/// Игровые экономические параметры, настраиваемые без пересборки кода.
/// </summary>
public sealed record EconomySettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Economy";

    /// <summary>
    /// Баланс золотых кредитов, начисляемый игроку при регистрации.
    /// </summary>
    public decimal StartingBalance { get; init; }
}
