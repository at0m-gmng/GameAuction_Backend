using GameBackend.ServiceDefaults.Observability;
using GameBackend.ServiceDefaults.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace GameBackend.ServiceDefaults.Extensions;

/// <summary>
/// Общая настройка хоста: логирование, CORS, контроллеры, документация API, служебные эндпоинты.
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Имя CORS-политики фронтенда.
    /// </summary>
    public const string FrontendCorsPolicy = "Frontend";

    /// <summary>
    /// Адрес фронтенда (GitHub Pages).
    /// </summary>
    public const string FrontendOrigin = "https://at0m-gmng.github.io";

    /// <summary>
    /// Путь проверки здоровья сервиса.
    /// </summary>
    public const string HealthPath = "/health";

    private const string AspNetCoreLogSource = "Microsoft.AspNetCore";

    /// <summary>
    /// Подключает Serilog со структурированным JSON-выводом в консоль.
    /// </summary>
    /// <param name="builder">Построитель приложения.</param>
    /// <returns>Тот же построитель.</returns>
    public static WebApplicationBuilder AddServiceLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override(AspNetCoreLogSource, LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter()));
        return builder;
    }

    /// <summary>
    /// Регистрирует CORS-политику фронтенда.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="allowCredentials">Разрешить credentials (нужно SignalR).</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, bool allowCredentials) =>
        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
        {
            policy.WithOrigins(FrontendOrigin).AllowAnyHeader().AllowAnyMethod();
            if (allowCredentials)
            {
                policy.AllowCredentials();
            }
        }));

    /// <summary>
    /// Регистрирует контроллеры с FluentValidation-валидаторами из сборки маркера.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <typeparam name="TValidatorMarker">Тип из сборки с валидаторами.</typeparam>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddValidatedControllers<TValidatorMarker>(this IServiceCollection services)
    {
        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining<TValidatorMarker>(services);
        services.AddControllers(options => options.Filters.Add<ValidationActionFilter>());
        return services;
    }

    /// <summary>
    /// Подключает correlation-id и логирование запросов в начало конвейера.
    /// </summary>
    /// <param name="app">Приложение.</param>
    /// <returns>То же приложение.</returns>
    public static WebApplication UseRequestObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        return app;
    }

    /// <summary>
    /// Публикует OpenAPI-документ и UI Scalar (/scalar/v1).
    /// </summary>
    /// <param name="app">Приложение.</param>
    /// <returns>То же приложение.</returns>
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }
}
