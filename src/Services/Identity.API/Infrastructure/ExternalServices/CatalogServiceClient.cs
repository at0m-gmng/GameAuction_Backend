using System.Net.Http.Json;

namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-реализация клиента к Catalog.API. BaseAddress и заголовок
/// X-Internal-Key настраиваются при регистрации HttpClient в Program.cs.
/// </summary>
public sealed class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;

    public CatalogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid> GrantItemAsync(Guid playerId, GeneratedItemResponse item, CancellationToken cancellationToken = default)
    {
        var request = new GrantItemRequest(
            playerId,
            item.Name,
            item.Description,
            item.Category,
            item.Rarity,
            item.ImageUrl,
            item.StartingPrice);

        var response = await _httpClient.PostAsJsonAsync("api/catalog/internal/grant-item", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }
}
