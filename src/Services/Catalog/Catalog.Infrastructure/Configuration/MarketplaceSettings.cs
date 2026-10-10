namespace GameBackend.Services.Catalog.Infrastructure.Configuration;

/// <summary>
/// Параметры генерации публичной витрины каталога, настраиваемые без пересборки кода.
/// </summary>
public sealed record MarketplaceSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Marketplace";

    private const int DefaultPublicCatalogSeedCount = 8;

    /// <summary>
    /// Сколько публичных предметов должно одновременно быть в витрине; продажи пополняются до этого числа.
    /// </summary>
    public int PublicCatalogSeedCount { get; init; } = DefaultPublicCatalogSeedCount;
}
