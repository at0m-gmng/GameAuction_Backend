using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GameBackend.Services.Identity.API.Extensions;

/// <summary>
/// Лимит запросов на вход и регистрацию — защита от перебора паролей.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Имя политики лимита для эндпоинтов аутентификации.
    /// </summary>
    public const string AuthPolicy = "auth";

    private const string ForwardedForHeader = "X-Forwarded-For";
    private const string UnknownClient = "unknown";
    private const char ForwardedForSeparator = ',';
    private const int PermitLimit = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Регистрирует фиксированное окно на IP клиента; IP берётся из X-Forwarded-For (за прокси Render).
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                ResolveClientIp(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = PermitLimit,
                    Window = Window,
                }));
        });

    private static string ResolveClientIp(HttpContext httpContext)
    {
        string? forwardedIp = httpContext.Request.Headers[ForwardedForHeader].FirstOrDefault()?.Split(ForwardedForSeparator)[0].Trim();
        return string.IsNullOrEmpty(forwardedIp)
            ? httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClient
            : forwardedIp;
    }
}
