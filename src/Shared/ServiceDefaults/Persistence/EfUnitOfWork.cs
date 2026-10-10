using GameBackend.SharedKernel.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameBackend.ServiceDefaults.Persistence;

/// <summary>
/// Транзакция EF Core внутри стратегии выполнения (обязательно при EnableRetryOnFailure).
/// </summary>
/// <typeparam name="TContext">Контекст сервиса.</typeparam>
public sealed class EfUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;

    /// <summary>
    /// Инициализирует единицу работы контекстом сервиса.
    /// </summary>
    /// <param name="context">Контекст сервиса.</param>
    public EfUnitOfWork(TContext context) => _context = context;

    /// <inheritdoc />
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            TResult result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken) =>
        await ExecuteInTransactionAsync(async token =>
        {
            await operation(token);
            return true;
        }, cancellationToken);
}
