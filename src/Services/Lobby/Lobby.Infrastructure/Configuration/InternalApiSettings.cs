using System.ComponentModel.DataAnnotations;
using GameBackend.ServiceDefaults.Security;

namespace GameBackend.Services.Lobby.Infrastructure.Configuration;

/// <summary>
/// Настройки межсервисных вызовов: общий секрет и адреса зависимых сервисов.
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

    /// <summary>
    /// Базовый адрес Identity.API.
    /// </summary>
    [Required(ErrorMessage = "InternalApi:IdentityBaseUrl не задан.")]
    public string IdentityBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Базовый адрес Catalog.API.
    /// </summary>
    [Required(ErrorMessage = "InternalApi:CatalogBaseUrl не задан.")]
    public string CatalogBaseUrl { get; init; } = string.Empty;
}
