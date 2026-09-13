namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Identity.API.
/// </summary>
public interface IIdentityServiceClient
{
    /// <summary>
    /// Списывает средства с баланса игрока при прямой покупке в каталоге.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если средств недостаточно или Identity.API недоступен.</exception>
    Task DebitAsync(Guid playerId, decimal amount, CancellationToken cancellationToken = default);
}
