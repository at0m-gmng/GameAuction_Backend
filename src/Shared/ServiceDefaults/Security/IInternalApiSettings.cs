namespace GameBackend.ServiceDefaults.Security;

/// <summary>
/// Настройки межсервисных вызовов, общие для всех сервисов.
/// </summary>
public interface IInternalApiSettings
{
    /// <summary>
    /// Общий секретный ключ (заголовок X-Internal-Key) для входящих и исходящих вызовов.
    /// </summary>
    string Key { get; }
}
