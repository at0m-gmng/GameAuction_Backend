using GameBackend.Services.Catalog.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных каталога. Точка доступа к таблицам через EF Core.
/// </summary>
public class CatalogDbContext : DbContext
{
    /// <summary>
    /// Инициализирует контекст с указанными опциями.
    /// </summary>
    /// <param name="options">Опции подключения к БД.</param>
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Таблица каталожных карточек предметов.
    /// </summary>
    public DbSet<Item> Items => Set<Item>();

    /// <summary>
    /// Таблица позиций инвентаря игроков.
    /// </summary>
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    /// <summary>
    /// Применяет все конфигурации маппинга из этой сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}