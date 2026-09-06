using GameBackend.Services.Generation.API.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GameBackend.Services.Generation.API.Infrastructure.Persistence;

/// <summary>
/// БД генерации: архетипы предметов и уровни редкости.
/// </summary>
public class GenerationDbContext : DbContext
{
    /// <summary>
    /// Инициализирует контекст с указанными опциями.
    /// </summary>
    /// <param name="options">Опции подключения к БД.</param>
    public GenerationDbContext(DbContextOptions<GenerationDbContext> options) : base(options) { }

    /// <summary>
    /// Таблица архетипов предметов.
    /// </summary>
    public DbSet<ItemArchetype> ItemArchetypes => Set<ItemArchetype>();

    /// <summary>
    /// Таблица уровней редкости.
    /// </summary>
    public DbSet<RarityTier> RarityTiers => Set<RarityTier>();

    /// <summary>
    /// Применяет конфигурации маппинга из этой сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemArchetype>(b =>
        {
            b.ToTable("ItemArchetypes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Category).IsRequired();
            b.Property(x => x.ImageUrl).HasMaxLength(2000);
            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<RarityTier>(b =>
        {
            b.ToTable("RarityTiers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Rarity).IsRequired();
            b.Property(x => x.PriceMultiplier).HasPrecision(18, 2);
            b.Ignore(x => x.DomainEvents);
        });

        base.OnModelCreating(modelBuilder);
    }
}