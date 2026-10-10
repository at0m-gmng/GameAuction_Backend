using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Generation.Application.Interfaces;
using GameBackend.Services.Generation.Infrastructure.Configuration;
using GameBackend.Services.Generation.Infrastructure.Persistence;
using GameBackend.Services.Generation.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Generation.Infrastructure;

/// <summary>
/// Регистрация слоя Infrastructure сервиса генерации.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionName = "GenerationDb";

    /// <summary>
    /// Регистрирует БД, репозиторий пулов и проверку входящего межсервисного ключа.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddGenerationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<GenerationDbContext>(configuration, ConnectionName);
        services.AddScoped<IGenerationPoolRepository, GenerationPoolRepository>();
        services.AddInternalApi<InternalApiSettings>(InternalApiSettings.SectionName);
        return services;
    }
}
