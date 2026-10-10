using GameBackend.Services.Identity.Application.Commands;
using GameBackend.Services.Identity.Application.Queries;
using GameBackend.Services.Identity.Application.Services;
using GameBackend.SharedKernel.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Identity.Application;

/// <summary>
/// Регистрация слоя Application сервиса идентификации.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует обработчики команд и запросов и прикладные сервисы.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterCommand, string>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, string>, LoginCommandHandler>();
        services.AddScoped<DebitBalanceCommandHandler>();
        services.AddScoped<CreditBalanceCommandHandler>();
        services.AddScoped<GetPlayerBalanceQueryHandler>();
        services.AddScoped<WelcomeGiftFulfiller>();
        return services;
    }
}
