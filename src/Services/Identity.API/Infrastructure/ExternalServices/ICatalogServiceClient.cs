namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Catalog.API.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Выдаёт игроку приватный предмет и возвращает идентификатор карточки.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Catalog.API недоступен или вернул ошибку.</exception>
    Task<Guid> GrantItemAsync(Guid playerId, GeneratedItemResponse item, CancellationToken cancellationToken = default);
}
