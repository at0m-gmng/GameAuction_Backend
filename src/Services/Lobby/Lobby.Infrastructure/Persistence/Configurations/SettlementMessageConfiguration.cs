using GameBackend.Services.Lobby.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Lobby.Infrastructure.Persistence.Configurations;

/// <summary>
/// Маппинг outbox расчётов на таблицу SettlementMessages.
/// </summary>
public sealed class SettlementMessageConfiguration : IEntityTypeConfiguration<SettlementMessage>
{
    /// <summary>
    /// Имя таблицы outbox.
    /// </summary>
    public const string TableName = "SettlementMessages";

    private const int AmountPrecision = 18;
    private const int AmountScale = 2;

    /// <summary>
    /// Настраивает таблицу: один расчёт на лобби, индекс по ожидающим доставки.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<SettlementMessage> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(AmountPrecision, AmountScale);
        builder.Property(x => x.LastError).HasMaxLength(SettlementMessage.LastErrorMaxLength);
        builder.Ignore(x => x.IsParked);
        builder.HasIndex(x => x.LobbyId).IsUnique();
        builder.HasIndex(x => new { x.ProcessedAt, x.NextAttemptAt });
    }
}
