using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Catalog.Application.Interfaces;
using GameBackend.Services.Catalog.Infrastructure.Configuration;
using GameBackend.Services.Catalog.Infrastructure.ExternalServices;
using GameBackend.Services.Catalog.Infrastructure.Persistence;
using GameBackend.Services.Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Catalog.Infrastructure;

/// <summary>
/// Регистрация слоя Infrastructure сервиса каталога.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionName = "CatalogDb";

    /// <summary>
    /// Регистрирует БД, репозитории, настройки витрины и клиенты Generation/Lobby/Identity.
    /// Списание за покупку не идемпотентно — клиент Identity работает без повторов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MarketplaceSettings>().BindConfiguration(MarketplaceSettings.SectionName);

        services.AddPostgresDbContext<CatalogDbContext>(configuration, ConnectionName);
        services.AddUnitOfWork<CatalogDbContext>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        services.AddInternalApi<InternalApiSettings>(InternalApiSettings.SectionName);
        services.AddInternalServiceClient<IGenerationServiceClient, GenerationServiceClient, InternalApiSettings>(settings => settings.GenerationBaseUrl)
            .AddColdStartResilience();
        services.AddInternalServiceClient<ILobbyServiceClient, LobbyServiceClient, InternalApiSettings>(settings => settings.LobbyBaseUrl)
            .AddStandardResilienceHandler();
        services.AddInternalServiceClient<IIdentityServiceClient, IdentityServiceClient, InternalApiSettings>(settings => settings.IdentityBaseUrl)
            .AddResilienceWithoutRetry();
        return services;
    }
}
