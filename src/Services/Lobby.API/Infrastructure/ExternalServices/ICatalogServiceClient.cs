namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Catalog.API.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Передаёт предмет победителю аукциона и переписывает цену каталога на цену продажи.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Catalog.API недоступен или вернул ошибку.</exception>
    Task AwardItemAsync(Guid itemId, Guid winnerId, decimal price, CancellationToken cancellationToken = default);
}
