namespace GameBackend.Services.Identity.API.Infrastructure.Security;

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
    public string Issuer { get; init; } = default!;

    /// <summary>
    /// Аудитория токена.
    /// </summary>
    public string Audience { get; init; } = default!;

    /// <summary>
    /// Секретный ключ для подписи токена.
    /// </summary>
    public string SecretKey { get; init; } = default!;

    /// <summary>
    /// Время жизни токена в минутах.
    /// </summary>
    public int ExpiryMinutes { get; init; } = 60;
}
