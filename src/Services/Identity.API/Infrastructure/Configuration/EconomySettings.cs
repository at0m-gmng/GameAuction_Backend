namespace GameBackend.Services.Identity.API.Infrastructure.Configuration;

/// <summary>
/// Игровые экономические параметры, настраиваемые без пересборки кода.
/// </summary>
public sealed class EconomySettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Economy";

    /// <summary>
    /// Баланс золотых кредитов, начисляемый игроку при регистрации.
    /// </summary>
    public decimal StartingBalance { get; set; }
}
