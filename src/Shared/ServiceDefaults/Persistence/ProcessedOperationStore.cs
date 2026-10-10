using GameBackend.SharedKernel.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.ServiceDefaults.Persistence;

/// <summary>
/// EF-реализация журнала идемпотентности поверх контекста сервиса.
/// </summary>
/// <typeparam name="TContext">Контекст сервиса.</typeparam>
public sealed class ProcessedOperationStore<TContext> : IProcessedOperationStore where TContext : ServiceDbContext
{
    private readonly TContext _context;

    /// <summary>
    /// Инициализирует журнал контекстом сервиса.
    /// </summary>
    /// <param name="context">Контекст сервиса.</param>
    public ProcessedOperationStore(TContext context) => _context = context;

    /// <inheritdoc />
    public async Task<ProcessedOperationRecord?> FindAsync(string key, CancellationToken cancellationToken) =>
        await _context.ProcessedOperations
            .Where(x => x.Key == key)
            .Select(x => new ProcessedOperationRecord(x.Result))
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public void Add(string key, string? result = null) => _context.ProcessedOperations.Add(new ProcessedOperation(key, result));
}
