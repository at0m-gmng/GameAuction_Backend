namespace GameBackend.Services.Lobby.API.Infrastructure.Configuration;

/// <summary>
/// Настройки межсервисных вызовов: общий секрет и адреса зависимых сервисов.
/// </summary>
public sealed class InternalApiSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "InternalApi";

    /// <summary>
    /// Общий секретный ключ (заголовок X-Internal-Key) для входящих и исходящих вызовов.
    /// </summary>
    public string Key { get; set; } = default!;

    /// <summary>
    /// Базовый адрес Identity.API.
    /// </summary>
    public string IdentityBaseUrl { get; set; } = default!;

    /// <summary>
    /// Базовый адрес Catalog.API.
    /// </summary>
    public string CatalogBaseUrl { get; set; } = default!;
}
