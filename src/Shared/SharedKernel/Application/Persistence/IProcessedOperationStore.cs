namespace GameBackend.SharedKernel.Application.Persistence;

/// <summary>
/// Журнал выполненных операций для идемпотентности межсервисных мутаций.
/// </summary>
public interface IProcessedOperationStore
{
    /// <summary>
    /// Ищет запись об уже выполненной операции по ключу.
    /// </summary>
    /// <param name="key">Идемпотентный ключ.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Запись операции или null, если ключ ещё не обрабатывался.</returns>
    Task<ProcessedOperationRecord?> FindAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет ключ в журнал; запись сохраняется вместе со следующим сохранением изменений.
    /// </summary>
    /// <param name="key">Идемпотентный ключ.</param>
    /// <param name="result">Результат операции для повтора, если есть.</param>
    void Add(string key, string? result = null);
}
