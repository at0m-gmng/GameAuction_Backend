namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Catalog.API.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Выдаёт игроку приватный предмет идемпотентно по ключу и возвращает идентификатор карточки.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Catalog.API недоступен или вернул ошибку.</exception>
    Task<Guid> GrantItemAsync(Guid playerId, GeneratedItemResponse item, string idempotencyKey, CancellationToken cancellationToken = default);
}
