using System.Text;
using GameBackend.ServiceDefaults.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameBackend.ServiceDefaults.Extensions;

/// <summary>
/// Общая JWT-аутентификация: токены выпускает Identity.API, остальные сервисы их проверяют.
/// </summary>
public static class AuthenticationExtensions
{
    private const string ShortKeyMessage = "Jwt:SecretKey слишком короткий для HS256: нужно минимум 32 байта.";

    /// <summary>
    /// Регистрирует <see cref="JwtSettings"/> с проверкой при старте, JWT Bearer-аутентификацию и авторизацию.
    /// Claim "sub" не переименовывается в легаси ClaimTypes.NameIdentifier.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configureBearer">Дополнительная настройка Bearer (например, токен SignalR из query).</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddGameJwtAuthentication(this IServiceCollection services, Action<JwtBearerOptions>? configureBearer = null)
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .Validate(settings => Encoding.UTF8.GetByteCount(settings.SecretKey) >= JwtSettings.HmacSha256MinKeyBytes, ShortKeyMessage)
            .ValidateOnStart();
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<JwtSettings>>().Value);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                JwtSettings jwt = jwtOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                };
                configureBearer?.Invoke(options);
            });

        services.AddAuthorization();
        return services;
    }
}
