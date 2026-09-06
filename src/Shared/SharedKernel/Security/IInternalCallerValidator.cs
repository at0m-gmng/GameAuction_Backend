namespace GameBackend.SharedKernel.Security;

/// <summary>
/// Проверяет, что вызывающая сторона — другой сервис backend'а, а не внешний клиент.
/// </summary>
public interface IInternalCallerValidator
{
    /// <summary>
    /// Сверяет присланный ключ (заголовок X-Internal-Key) с общим секретом.
    /// </summary>
    bool IsValid(string? providedKey);
}
