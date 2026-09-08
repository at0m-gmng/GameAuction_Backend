namespace GameBackend.Services.Generation.API.Infrastructure.Configuration;

/// <summary>
/// Настройки межсервисных вызовов: общий секрет для проверки входящих запросов.
/// </summary>
public sealed class InternalApiSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "InternalApi";

    /// <summary>
    /// Общий секретный ключ (заголовок X-Internal-Key) для входящих вызовов.
    /// </summary>
    public string Key { get; set; } = default!;
}
