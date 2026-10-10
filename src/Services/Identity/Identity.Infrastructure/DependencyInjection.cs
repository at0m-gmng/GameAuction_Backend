using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Identity.Application.Configuration;
using GameBackend.Services.Identity.Application.Interfaces;
using GameBackend.Services.Identity.Infrastructure.Configuration;
using GameBackend.Services.Identity.Infrastructure.ExternalServices;
using GameBackend.Services.Identity.Infrastructure.Persistence;
using GameBackend.Services.Identity.Infrastructure.Persistence.Repositories;
using GameBackend.Services.Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameBackend.Services.Identity.Infrastructure;

/// <summary>
/// Регистрация слоя Infrastructure сервиса идентификации.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionName = "IdentityDb";

    /// <summary>
    /// Регистрирует БД, репозитории, безопасность, настройки и клиенты Generation/Catalog.
    /// Секция Economy необязательна: это игровой параметр, а не секрет; без неё StartingBalance = 0.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EconomySettings>().BindConfiguration(EconomySettings.SectionName);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<EconomySettings>>().Value);

        services.AddPostgresDbContext<IdentityDbContext>(configuration, ConnectionName);
        services.AddUnitOfWork<IdentityDbContext>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        services.AddInternalApi<InternalApiSettings>(InternalApiSettings.SectionName);
        services.AddInternalServiceClient<IGenerationServiceClient, GenerationServiceClient, InternalApiSettings>(settings => settings.GenerationBaseUrl)
            .AddColdStartResilience();
        services.AddInternalServiceClient<ICatalogServiceClient, CatalogServiceClient, InternalApiSettings>(settings => settings.CatalogBaseUrl)
            .AddStandardResilienceHandler();
        return services;
    }
}
