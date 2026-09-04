using System.Text;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GameBackend.Services.Identity.API.Infrastructure.Security;

/// <summary>
/// Регистрирует всё, что связано с JWT: настройки, генератор токенов (выпуск) и
/// проверку входящих токенов (валидация). Выпуск и валидация настраиваются из
/// одного набора <see cref="JwtSettings"/>, поэтому их параметры (issuer,
/// audience, ключ) не могут разойтись.
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>HS256 требует ключ не короче 256 бит (32 байта).</summary>
    private const int HmacSha256MinKeyBytes = 32;

    /// <summary>
    /// Настраивает выпуск и валидацию JWT на основе секции <c>Jwt</c> конфигурации.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция сервисов для сцепления вызовов.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Конфигурация JWT отсутствует: секция '{JwtSettings.SectionName}' не найдена.");

        ValidateOrThrow(settings);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));

        services.AddSingleton(settings);
        services.AddSingleton<JwtTokenGenerator>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Проверяет конфигурацию на старте, чтобы кривой деплой падал сразу и с
    /// понятным сообщением, а не отвечал молчаливым 401 на каждый запрос.
    /// </summary>
    private static void ValidateOrThrow(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer))
            throw new InvalidOperationException("Jwt:Issuer не задан.");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("Jwt:Audience не задан.");

        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException(
                "Jwt:SecretKey не задан — установите переменную окружения Jwt__SecretKey.");

        if (Encoding.UTF8.GetByteCount(settings.SecretKey) < HmacSha256MinKeyBytes)
            throw new InvalidOperationException(
                $"Jwt:SecretKey слишком короткий для HS256: нужно минимум {HmacSha256MinKeyBytes} байт.");
    }
}
