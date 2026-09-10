namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Identity.API.
/// </summary>
public interface IIdentityServiceClient
{
    /// <summary>
    /// Списывает сумму с баланса игрока.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task DebitAsync(Guid playerId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пополняет баланс игрока (например, получение средств от продажи на аукционе).
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task CreditAsync(Guid playerId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает текущий баланс игрока.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Identity.API недоступен или вернул ошибку.</exception>
    Task<decimal> GetBalanceAsync(Guid playerId, CancellationToken cancellationToken = default);
}
