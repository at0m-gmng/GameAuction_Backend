using System.ComponentModel.DataAnnotations;
using GameBackend.ServiceDefaults.Security;

namespace GameBackend.Services.Generation.Infrastructure.Configuration;

/// <summary>
/// Настройки межсервисных вызовов: общий секрет.
/// </summary>
public sealed record InternalApiSettings : IInternalApiSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "InternalApi";

    /// <summary>
    /// Общий секретный ключ (заголовок X-Internal-Key) для входящих и исходящих вызовов.
    /// </summary>
    [Required(ErrorMessage = "InternalApi:Key не задан — установите переменную окружения InternalApi__Key.")]
    public string Key { get; init; } = string.Empty;
}
