using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.ServiceDefaults.Persistence;

/// <summary>
/// Маппинг журнала идемпотентности на таблицу ProcessedOperations.
/// </summary>
public sealed class ProcessedOperationConfiguration : IEntityTypeConfiguration<ProcessedOperation>
{
    /// <summary>
    /// Имя таблицы журнала.
    /// </summary>
    public const string TableName = "ProcessedOperations";

    /// <summary>
    /// Настраивает таблицу: ключ операции — первичный ключ.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<ProcessedOperation> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
