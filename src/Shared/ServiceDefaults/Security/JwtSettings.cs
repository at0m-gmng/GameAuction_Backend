using System.ComponentModel.DataAnnotations;

namespace GameBackend.ServiceDefaults.Security;

/// <summary>
/// Настройки выпуска и проверки JWT-токенов. Issuer/Audience/SecretKey должны совпадать во всех сервисах.
/// </summary>
public sealed record JwtSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Минимальная длина ключа HS256 в байтах.
    /// </summary>
    public const int HmacSha256MinKeyBytes = 32;

    private const int DefaultExpiryMinutes = 60;

    /// <summary>
    /// Издатель токена.
    /// </summary>
    [Required(ErrorMessage = "Jwt:Issuer не задан.")]
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Аудитория токена.
    /// </summary>
    [Required(ErrorMessage = "Jwt:Audience не задан.")]
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Секретный ключ для подписи токена.
    /// </summary>
    [Required(ErrorMessage = "Jwt:SecretKey не задан — установите переменную окружения Jwt__SecretKey.")]
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Время жизни токена в минутах. 60-минутная сессия в localStorage — осознанный выбор демо (ADR 0007).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ExpiryMinutes { get; init; } = DefaultExpiryMinutes;
}
