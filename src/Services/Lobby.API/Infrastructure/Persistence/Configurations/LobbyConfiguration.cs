using GameBackend.Services.Lobby.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Lobby.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга агрегата Lobby на таблицу Lobbies.
/// </summary>
public class LobbyConfiguration : IEntityTypeConfiguration<LobbyAggregate>
{
    private const int ItemNameMaxLength = 200;
    private const int ImageUrlMaxLength = 2000;
    private const int PricePrecision = 18;
    private const int PriceScale = 2;

    /// <summary>
    /// Настраивает маппинг агрегата Lobby.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<LobbyAggregate> builder)
    {
        builder.ToTable("Lobbies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemId).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(ItemNameMaxLength).IsRequired();
        builder.Property(x => x.ItemImageUrl).HasMaxLength(ImageUrlMaxLength);
        builder.Property(x => x.StartingPrice).HasPrecision(PricePrecision, PriceScale);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.MaxParticipants).IsRequired();

        // Участники — массив uuid[] в PostgreSQL (primitive collection).
        builder.Ignore(x => x.Participants);
        builder.PrimitiveCollection<List<Guid>>("_participants")
            .HasColumnName("Participants");

        // Ставки — дочерняя коллекция в отдельной таблице.
        builder.OwnsMany(x => x.Bids, bids =>
        {
            bids.ToTable("Bids");
            bids.WithOwner().HasForeignKey("LobbyId");
            bids.HasKey(x => x.Id);
            bids.Property(x => x.PlayerId).IsRequired();
            bids.Property(x => x.Amount).HasPrecision(PricePrecision, PriceScale);
            bids.Property(x => x.PlacedAt).IsRequired();
        });

        // Вычисляемое — не персистим.
        builder.Ignore(x => x.CurrentBid);

        // Индекс ускоряет выборку открытых лобби.
        builder.HasIndex(x => x.Status);
    }
}