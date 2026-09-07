namespace GameBackend.SharedKernel.Security;

/// <summary>
/// Настройки выпуска JWT-токенов.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Имя секции в конфигурации (appsettings.json).
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Издатель токена.
    /// </summary>
    public string Issuer { get; set; } = default!;

    /// <summary>
    /// Аудитория токена.
    /// </summary>
    public string Audience { get; set; } = default!;

    /// <summary>
    /// Секретный ключ для подписи токена.
    /// </summary>
    public string SecretKey { get; set; } = default!;

    /// <summary>
    /// Время жизни токена в минутах.
    /// </summary>
    // TODO: 60-минутная сессия в localStorage — нужен короткий access + refresh в httpOnly-cookie.
    public int ExpiryMinutes { get; set; } = 60;
}
