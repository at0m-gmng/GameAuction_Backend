using GameBackend.ServiceDefaults.Observability;
using GameBackend.ServiceDefaults.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace GameBackend.ServiceDefaults.Extensions;

/// <summary>
/// Межсервисные вызовы: настройки с проверкой при старте, проверка входящего ключа, типизированные HTTP-клиенты.
/// </summary>
public static class InternalApiExtensions
{
    private static readonly TimeSpan ColdStartAttemptTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ColdStartTotalTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan ColdStartSamplingDuration = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Регистрирует настройки межсервисных вызовов с проверкой при старте и валидатор входящего ключа.
    /// Тот же ключ используется и для входящих проверок, и для исходящих вызовов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="sectionName">Секция конфигурации.</param>
    /// <typeparam name="TSettings">Тип настроек сервиса.</typeparam>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddInternalApi<TSettings>(this IServiceCollection services, string sectionName)
        where TSettings : class, IInternalApiSettings
    {
        services.AddOptions<TSettings>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IInternalCallerValidator>(provider =>
            new InternalCallerValidator(provider.GetRequiredService<IOptions<TSettings>>().Value.Key));
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        return services;
    }

    /// <summary>
    /// Регистрирует типизированный клиент другого сервиса: базовый адрес, общий ключ и проброс correlation-id.
    /// Resilience-пайплайн подключает вызывающий код.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="baseUrlSelector">Выбор базового адреса из настроек.</param>
    /// <typeparam name="TClient">Интерфейс клиента.</typeparam>
    /// <typeparam name="TImplementation">Реализация клиента.</typeparam>
    /// <typeparam name="TSettings">Тип настроек сервиса.</typeparam>
    /// <returns>Построитель HTTP-клиента.</returns>
    public static IHttpClientBuilder AddInternalServiceClient<TClient, TImplementation, TSettings>(
        this IServiceCollection services,
        Func<TSettings, string> baseUrlSelector)
        where TClient : class
        where TImplementation : class, TClient
        where TSettings : class, IInternalApiSettings =>
        services.AddHttpClient<TClient, TImplementation>((provider, client) =>
            {
                TSettings settings = provider.GetRequiredService<IOptions<TSettings>>().Value;
                client.BaseAddress = new Uri(baseUrlSelector(settings));
                client.DefaultRequestHeaders.Add(InternalApiHeaders.InternalKey, settings.Key);
            })
            .AddHttpMessageHandler<CorrelationIdHandler>();

    /// <summary>
    /// Resilience под холодный старт free-tier: retry будит уснувший сервис, attempt-timeout увеличен.
    /// </summary>
    /// <param name="builder">Построитель HTTP-клиента.</param>
    /// <returns>Тот же построитель.</returns>
    public static IHttpClientBuilder AddColdStartResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = ColdStartAttemptTimeout;
            options.TotalRequestTimeout.Timeout = ColdStartTotalTimeout;
            options.CircuitBreaker.SamplingDuration = ColdStartSamplingDuration;
        });
        return builder;
    }

    /// <summary>
    /// Resilience без повторов (timeout + circuit breaker) для неидемпотентных вызовов.
    /// </summary>
    /// <param name="builder">Построитель HTTP-клиента.</param>
    /// <returns>Тот же построитель.</returns>
    public static IHttpClientBuilder AddResilienceWithoutRetry(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options => options.Retry.ShouldHandle = _ => ValueTask.FromResult(false));
        return builder;
    }
}
