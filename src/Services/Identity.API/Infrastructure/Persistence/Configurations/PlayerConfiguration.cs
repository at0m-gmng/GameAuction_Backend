using GameBackend.Services.Identity.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Identity.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга агрегата Player на таблицу Players.
/// </summary>
public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    private const int NicknameMaxLength = 100;
    private const int EmailMaxLength = 256;
    private const int PasswordHashMaxLength = 512;

    /// <summary>
    /// Настраивает маппинг агрегата Player.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nickname).HasMaxLength(NicknameMaxLength).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(EmailMaxLength).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(EmailMaxLength).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(PasswordHashMaxLength).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Уникальный индекс по нормализованному email для быстрого поиска и уникальности.
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();

        // Инвентарь — массив uuid[] в PostgreSQL (primitive collection).
        builder.Ignore(x => x.Inventory);
        builder.PrimitiveCollection<List<Guid>>("_inventory")
            .HasColumnName("Inventory");

        // GoldCredits — owned type (value object).
        builder.OwnsOne(x => x.Balance, balance =>
        {
            balance.Property(x => x.Amount).HasColumnName("BalanceAmount");
        });
    }
}