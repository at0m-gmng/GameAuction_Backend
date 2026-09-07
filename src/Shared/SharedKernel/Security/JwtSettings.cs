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
    // TODO: сейчас единственный токен (и access, и de-facto "сессия") живёт
    // 60 минут, а хранится на фронте в localStorage — значит либо пользователь
    // логинится каждый час, либо (быстрый, не best-practice вариант) продлеваем
    // ExpiryMinutes на 30 дней и держим riск дольше живущего токена в XSS-уязвимом
    // хранилище. Правильный вариант — короткий access-токен (5-15 мин) + отдельный
    // refresh-токен (7-30 дней) в httpOnly-cookie с ротацией. Обсуждено с пользователем,
    // решение отложено до отдельной уборки TODO.
    public int ExpiryMinutes { get; set; } = 60;
}
