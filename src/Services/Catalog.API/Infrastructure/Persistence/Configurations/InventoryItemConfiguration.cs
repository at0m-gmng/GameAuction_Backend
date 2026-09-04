using GameBackend.Services.Catalog.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Маппинг позиции инвентаря на таблицу InventoryItems.
/// </summary>
public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    /// <summary>Настраивает маппинг позиции инвентаря.</summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlayerId).IsRequired();
        builder.Property(x => x.ItemId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.AcquiredAt).IsRequired();

        // У игрока ровно одна строка на тип предмета.
        builder.HasIndex(x => new { x.PlayerId, x.ItemId }).IsUnique();

        // Доменные события не сохраняются в БД.
        builder.Ignore(x => x.DomainEvents);
    }
}