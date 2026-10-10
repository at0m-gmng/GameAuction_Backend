using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Infrastructure.Configuration;
using GameBackend.Services.Lobby.Infrastructure.ExternalServices;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using GameBackend.Services.Lobby.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Lobby.Infrastructure;

/// <summary>
/// Регистрация слоя Infrastructure сервиса лобби.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionName = "LobbyDb";

    /// <summary>
    /// Регистрирует БД, репозиторий и клиенты Identity/Catalog.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddLobbyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<LobbyDbContext>(configuration, ConnectionName);
        services.AddScoped<ILobbyRepository, LobbyRepository>();

        services.AddInternalApi<InternalApiSettings>(InternalApiSettings.SectionName);
        services.AddInternalServiceClient<IIdentityServiceClient, IdentityServiceClient, InternalApiSettings>(settings => settings.IdentityBaseUrl)
            .AddStandardResilienceHandler();
        services.AddInternalServiceClient<ICatalogServiceClient, CatalogServiceClient, InternalApiSettings>(settings => settings.CatalogBaseUrl)
            .AddStandardResilienceHandler();
        return services;
    }
}
