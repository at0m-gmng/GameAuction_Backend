using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.API.Realtime;
using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.SharedKernel.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace GameBackend.Services.Lobby.API.Extensions;

/// <summary>
/// Realtime-слой лобби: SignalR-хаб, доставка доменных событий и адресные уведомления.
/// </summary>
public static class RealtimeExtensions
{
    /// <summary>
    /// Путь SignalR-хаба лобби.
    /// </summary>
    public const string LobbyHubPath = "/hubs/lobby";

    private const string AccessTokenQueryKey = "access_token";

    /// <summary>
    /// Регистрирует SignalR, провайдер id пользователя (claim "sub"), диспетчер событий и уведомитель.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddLobbyRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubjectUserIdProvider>();
        services.AddScoped<IDomainEventDispatcher, SignalRDomainEventDispatcher>();
        services.AddScoped<IPlayerNotifier, SignalRPlayerNotifier>();
        return services;
    }

    /// <summary>
    /// WebSocket не шлёт заголовок Authorization — токен хаба берётся из query-string access_token.
    /// </summary>
    /// <param name="options">Опции JWT Bearer.</param>
    public static void ReadHubTokenFromQuery(JwtBearerOptions options) =>
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                StringValues accessToken = context.Request.Query[AccessTokenQueryKey];
                if (!StringValues.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(LobbyHubPath))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
}
