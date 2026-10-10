using GameBackend.Services.Lobby.Application.Commands;
using GameBackend.Services.Lobby.Application.Queries;
using GameBackend.Services.Lobby.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Lobby.Application;

/// <summary>
/// Регистрация слоя Application сервиса лобби.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует обработчики команд и запросов и расчёт аукциона.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddLobbyApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateLobbyCommandHandler>();
        services.AddScoped<JoinLobbyCommandHandler>();
        services.AddScoped<LeaveLobbyCommandHandler>();
        services.AddScoped<PlaceBidCommandHandler>();
        services.AddScoped<AuctionCompletionService>();
        services.AddScoped<GetOpenLobbiesQueryHandler>();
        services.AddScoped<GetLobbyQueryHandler>();
        services.AddScoped<GetPlayerAuctionStatsQueryHandler>();
        services.AddScoped<GetPlayerAuctionHistoryQueryHandler>();
        return services;
    }
}
