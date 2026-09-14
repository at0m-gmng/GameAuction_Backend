namespace GameBackend.Services.Catalog.API.Infrastructure.Persistence;

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
    /// Результат операции для повтора (например, продавец при выдаче); null, если результата нет.
    /// </summary>
    public string? Result { get; private set; }

    /// <summary>
    /// Момент обработки операции (UTC).
    /// </summary>
    public DateTime ProcessedAt { get; private set; }

    private ProcessedOperation()
    {
    }

    /// <summary>
    /// Создаёт запись журнала для ключа операции с необязательным результатом.
    /// </summary>
    /// <param name="key">Идемпотентный ключ.</param>
    /// <param name="result">Результат операции для повтора, если есть.</param>
    public ProcessedOperation(string key, string? result = null)
    {
        Key = key;
        Result = result;
        ProcessedAt = DateTime.UtcNow;
    }
}
