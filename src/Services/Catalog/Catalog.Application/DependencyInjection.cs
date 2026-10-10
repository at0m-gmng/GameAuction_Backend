using GameBackend.Services.Catalog.Application.Commands;
using GameBackend.Services.Catalog.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Catalog.Application;

/// <summary>
/// Регистрация слоя Application сервиса каталога.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует обработчики команд и запросов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateItemCommandHandler>();
        services.AddScoped<BuyItemCommandHandler>();
        services.AddScoped<AwardItemCommandHandler>();
        services.AddScoped<GrantItemCommandHandler>();
        services.AddScoped<ListInventoryItemForSaleCommandHandler>();
        services.AddScoped<UnlistItemCommandHandler>();
        services.AddScoped<StartAuctionCommandHandler>();
        services.AddScoped<GeneratePublicItemCommandHandler>();
        services.AddScoped<GetItemsQueryHandler>();
        services.AddScoped<GetInventoryQueryHandler>();
        services.AddScoped<GetMyListingsQueryHandler>();
        return services;
    }
}
