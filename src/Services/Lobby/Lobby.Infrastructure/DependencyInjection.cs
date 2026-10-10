using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Infrastructure.Background;
using GameBackend.Services.Lobby.Infrastructure.Configuration;
using GameBackend.Services.Lobby.Infrastructure.ExternalServices;
using GameBackend.Services.Lobby.Infrastructure.Outbox;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using GameBackend.Services.Lobby.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameBackend.Services.Lobby.Infrastructure;

/// <summary>
/// Регистрация слоя Infrastructure сервиса лобби.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionName = "LobbyDb";

    /// <summary>
    /// Регистрирует БД, репозиторий, outbox расчётов, фоновые обработчики (расчёты, просроченные аукционы) и клиенты Identity/Catalog.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddLobbyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<LobbyDbContext>(configuration, ConnectionName);
        services.AddScoped<ILobbyRepository, LobbyRepository>();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<SettlementOutboxSignal>();
        services.AddScoped<ISettlementOutbox, SettlementOutbox>();
        services.AddScoped<SettlementOutboxDispatcher>();
        services.AddHostedService<SettlementOutboxProcessor>();
        services.AddHostedService<AuctionExpiryProcessor>();

        services.AddInternalApi<InternalApiSettings>(InternalApiSettings.SectionName);
        services.AddInternalServiceClient<IIdentityServiceClient, IdentityServiceClient, InternalApiSettings>(settings => settings.IdentityBaseUrl)
            .AddStandardResilienceHandler();
        services.AddInternalServiceClient<ICatalogServiceClient, CatalogServiceClient, InternalApiSettings>(settings => settings.CatalogBaseUrl)
            .AddStandardResilienceHandler();
        return services;
    }
}
