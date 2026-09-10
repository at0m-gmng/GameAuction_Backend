using GameBackend.Services.Catalog.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга агрегата Item на таблицу Items.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    /// <summary>
    /// Настраивает маппинг агрегата Item.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ImageUrl).HasMaxLength(2000);
        builder.Property(x => x.StartingPrice).HasPrecision(18, 2);
        builder.Property(x => x.IsListed).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // NOTE: Rarity — int, а не enum-строка, чтобы фильтр ">=" сравнивал значения, а не строки.
        builder.Property(x => x.Rarity).IsRequired();

        // NOTE: Category — строка (сравнивается только на равенство), для читаемости в БД.
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.Rarity);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.IsListed);

        builder.Ignore(x => x.DomainEvents);
    }
}