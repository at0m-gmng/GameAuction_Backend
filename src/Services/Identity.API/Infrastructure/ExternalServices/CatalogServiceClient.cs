using System.Net.Http.Json;

namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-клиент к Catalog.API; BaseAddress и X-Internal-Key настраиваются в Program.cs.
/// </summary>
public sealed class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;

    public CatalogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid> GrantItemAsync(Guid playerId, GeneratedItemResponse item, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var request = new GrantItemRequest(
            playerId,
            item.Name,
            item.Description,
            item.Category,
            item.Rarity,
            item.ImageUrl,
            item.StartingPrice);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/catalog/internal/grant-item")
        {
            Content = JsonContent.Create(request),
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }
}
