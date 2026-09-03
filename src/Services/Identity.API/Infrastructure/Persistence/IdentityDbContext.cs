using GameBackend.Services.Identity.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Identity.API.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных для сервиса идентификации.
/// </summary>
public class IdentityDbContext : DbContext
{
    /// <summary>
    /// Инициализирует контекст с указанными опциями.
    /// </summary>
    /// <param name="options">Опции подключения к БД.</param>
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Таблица игроков.
    /// </summary>
    public DbSet<Player> Players => Set<Player>();

    /// <summary>
    /// Применяет конфигурации маппинга из сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}