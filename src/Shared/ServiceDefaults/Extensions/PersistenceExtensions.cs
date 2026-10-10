using GameBackend.ServiceDefaults.Persistence;
using GameBackend.SharedKernel.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.ServiceDefaults.Extensions;

/// <summary>
/// Подключение PostgreSQL (Neon) с повтором транзиентных ошибок и проверкой здоровья.
/// </summary>
public static class PersistenceExtensions
{
    private const int MaxRetryCount = 3;
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Регистрирует контекст PostgreSQL и health check по нему.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="connectionName">Имя строки подключения.</param>
    /// <typeparam name="TContext">Контекст сервиса.</typeparam>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddPostgresDbContext<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionName)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(connectionName),
                npgsql => npgsql.EnableRetryOnFailure(MaxRetryCount, MaxRetryDelay, null)));
        services.AddHealthChecks().AddDbContextCheck<TContext>();
        return services;
    }

    /// <summary>
    /// Регистрирует единицу работы и журнал идемпотентности поверх контекста сервиса.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <typeparam name="TContext">Контекст сервиса.</typeparam>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services) where TContext : ServiceDbContext
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork<TContext>>();
        services.AddScoped<IProcessedOperationStore, ProcessedOperationStore<TContext>>();
        return services;
    }
}
