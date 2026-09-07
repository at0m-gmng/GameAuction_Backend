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
    /// Сколько публичных предметов сгенерировать, если витрина пуста.
    /// </summary>
    public int PublicCatalogSeedCount { get; set; } = 8;

    /// <summary>
    /// Сколько предметов добавлять при очередном пополнении витрины.
    /// </summary>
    public int RestockBatchSize { get; set; } = 8;

    /// <summary>
    /// Минимальный интервал между пополнениями витрины.
    /// </summary>
    public int RestockIntervalHours { get; set; } = 24;
}
