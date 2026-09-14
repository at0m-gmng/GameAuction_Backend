using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBackend.Services.Identity.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Маппинг журнала идемпотентности на таблицу ProcessedOperations.
/// </summary>
public class ProcessedOperationConfiguration : IEntityTypeConfiguration<ProcessedOperation>
{
    /// <summary>
    /// Настраивает таблицу: ключ операции — первичный ключ.
    /// </summary>
    /// <param name="builder">Построитель сущности.</param>
    public void Configure(EntityTypeBuilder<ProcessedOperation> builder)
    {
        builder.ToTable("ProcessedOperations");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
