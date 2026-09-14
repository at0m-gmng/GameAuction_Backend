namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Identity.API.
/// </summary>
public interface IIdentityServiceClient
{
    /// <summary>
    /// Идемпотентно списывает сумму с баланса игрока по ключу операции.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task DebitAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Идемпотентно пополняет баланс игрока по ключу операции (средства от продажи на аукционе).
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task CreditAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает текущий баланс игрока.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task<decimal> GetBalanceAsync(Guid playerId, CancellationToken cancellationToken = default);
}
