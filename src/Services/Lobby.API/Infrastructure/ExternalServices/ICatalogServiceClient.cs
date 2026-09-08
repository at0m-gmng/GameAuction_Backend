namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Catalog.API.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Передаёт предмет победителю аукциона.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Catalog.API недоступен или вернул ошибку.</exception>
    Task AwardItemAsync(Guid itemId, Guid winnerId, CancellationToken cancellationToken = default);
}
