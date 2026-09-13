namespace GameBackend.Services.Catalog.API.Infrastructure.Configuration;

/// <summary>
/// Параметры генерации публичной витрины каталога, настраиваемые без пересборки кода.
/// </summary>
public sealed class MarketplaceSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Marketplace";

    /// <summary>
    /// Сколько публичных предметов должно одновременно быть в витрине; продажи пополняются до этого числа.
    /// </summary>
    public int PublicCatalogSeedCount { get; set; } = 8;
}
