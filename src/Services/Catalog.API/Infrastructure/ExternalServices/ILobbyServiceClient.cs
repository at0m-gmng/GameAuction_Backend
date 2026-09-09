using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Lobby.API.
/// </summary>
public interface ILobbyServiceClient
{
    /// <summary>
    /// Создаёт лобби для предмета.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Lobby.API недоступен или вернул ошибку.</exception>
    Task<Guid> CreateLobbyAsync(
        Guid itemId,
        string itemName,
        string? itemImageUrl,
        ItemRarity itemRarity,
        decimal startingPrice,
        int maxParticipants,
        CancellationToken cancellationToken = default);
}
