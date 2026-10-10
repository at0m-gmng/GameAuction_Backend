using GameBackend.Services.Generation.Application.Commands;
using GameBackend.Services.Generation.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Generation.Application;

/// <summary>
/// Регистрация слоя Application сервиса генерации.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует доменный генератор и обработчик команды генерации.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddGenerationApplication(this IServiceCollection services)
    {
        services.AddSingleton<IItemGenerator, ItemGenerator>();
        services.AddScoped<GenerateItemCommandHandler>();
        return services;
    }
}
