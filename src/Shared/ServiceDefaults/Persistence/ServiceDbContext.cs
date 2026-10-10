using GameBackend.SharedKernel.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.ServiceDefaults.Persistence;

/// <summary>
/// Базовый контекст сервиса: журнал идемпотентности и перевод нарушений уникальности в <see cref="DuplicateEntryException"/>.
/// </summary>
public abstract class ServiceDbContext : DbContext
{
    private const string PostgresDuplicateKeyMarker = "duplicate key";
    private const string UniqueConstraintMarker = "unique constraint";

    /// <summary>
    /// Инициализирует контекст с указанными опциями.
    /// </summary>
    /// <param name="options">Опции подключения к БД.</param>
    protected ServiceDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Журнал обработанных операций для идемпотентности.
    /// </summary>
    public DbSet<ProcessedOperation> ProcessedOperations => Set<ProcessedOperation>();

    /// <summary>
    /// Сохраняет изменения; нарушение уникальности пробрасывается как <see cref="DuplicateEntryException"/>.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Принять изменения трекера после успеха.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Число записанных строк.</returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateEntryException(ex);
        }
    }

    /// <summary>
    /// Применяет общую конфигурацию журнала идемпотентности.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProcessedOperationConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains(PostgresDuplicateKeyMarker, StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains(UniqueConstraintMarker, StringComparison.OrdinalIgnoreCase) == true;
}
