namespace GameBackend.Services.Identity.API.Infrastructure.Persistence;

/// <summary>
/// Журнал выполненных операций для идемпотентности: ключ уже обработан — повтор пропускается.
/// </summary>
public sealed class ProcessedOperation
{
    /// <summary>
    /// Идемпотентный ключ операции (первичный ключ).
    /// </summary>
    public string Key { get; private set; } = default!;

    /// <summary>
    /// Момент обработки операции (UTC).
    /// </summary>
    public DateTime ProcessedAt { get; private set; }

    private ProcessedOperation()
    {
    }

    /// <summary>
    /// Создаёт запись журнала для указанного ключа операции.
    /// </summary>
    /// <param name="key">Идемпотентный ключ.</param>
    public ProcessedOperation(string key)
    {
        Key = key;
        ProcessedAt = DateTime.UtcNow;
    }
}
