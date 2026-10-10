namespace GameBackend.ServiceDefaults.Security;

/// <summary>
/// Заголовки межсервисных вызовов.
/// </summary>
public static class InternalApiHeaders
{
    /// <summary>
    /// Общий секрет вызывающего сервиса.
    /// </summary>
    public const string InternalKey = "X-Internal-Key";

    /// <summary>
    /// Ключ идемпотентности мутирующей операции.
    /// </summary>
    public const string IdempotencyKey = "Idempotency-Key";
}
