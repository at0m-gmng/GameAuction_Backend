namespace GameBackend.SharedKernel.Application.Persistence;

/// <summary>
/// Выполняет набор изменений хранилища атомарно, в одной транзакции.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Выполняет операцию в транзакции; фиксирует при успешном завершении, откатывает при исключении.
    /// </summary>
    /// <param name="operation">Операция над хранилищем.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <typeparam name="TResult">Тип результата операции.</typeparam>
    /// <returns>Результат операции.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);

    /// <summary>
    /// Выполняет операцию без результата в транзакции; фиксирует при успехе, откатывает при исключении.
    /// </summary>
    /// <param name="operation">Операция над хранилищем.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
