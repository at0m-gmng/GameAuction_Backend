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

        // Редкость храним как число (int), чтобы корректно работал фильтр ">="
        // (сравнение идёт по числовому значению, а не по строке).
        builder.Property(x => x.Rarity).IsRequired();

        // Категория сравнивается только на равенство, поэтому строка допустима
        // и делает данные читаемыми в БД.
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);

        // Индексы ускоряют фильтрацию по категории и редкости.
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.Rarity);

        // Индекс по владельцу — по нему ищутся приватные предметы конкретного игрока.
        builder.HasIndex(x => x.OwnerId);

        // Доменные события живут только в памяти и не сохраняются в БД.
        builder.Ignore(x => x.DomainEvents);
    }
}