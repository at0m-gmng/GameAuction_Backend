using System.Net.Http.Json;

namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

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

    public async Task<Guid?> AwardItemAsync(Guid itemId, Guid winnerId, decimal price, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/catalog/internal/items/{itemId}/award",
            new { PlayerId = winnerId, Quantity = 1, Price = price },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid?>(cancellationToken: cancellationToken);
    }
}
