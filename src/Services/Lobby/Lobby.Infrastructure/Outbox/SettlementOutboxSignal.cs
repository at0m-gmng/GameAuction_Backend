namespace GameBackend.Services.Lobby.Infrastructure.Outbox;

/// <summary>
/// Будит обработчик outbox сразу после сохранения нового расчёта, не дожидаясь интервала опроса.
/// </summary>
public sealed class SettlementOutboxSignal : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    /// <summary>
    /// Сообщает, что в outbox появилась запись.
    /// </summary>
    public void Notify()
    {
        if (_semaphore.CurrentCount == 0)
        {
            try
            {
                _semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }

    /// <summary>
    /// Ждёт сигнала или истечения интервала.
    /// </summary>
    /// <param name="timeout">Максимальное ожидание.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true — пришёл сигнал, false — истёк интервал.</returns>
    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) => _semaphore.WaitAsync(timeout, cancellationToken);

    /// <inheritdoc />
    public void Dispose() => _semaphore.Dispose();
}
