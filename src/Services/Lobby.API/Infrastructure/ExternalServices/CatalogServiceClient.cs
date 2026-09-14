using System.Net.Http.Json;
using System.Text.Json;

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

    public async Task<Guid?> AwardItemAsync(Guid itemId, Guid winnerId, decimal price, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/catalog/internal/items/{itemId}/award")
        {
            Content = JsonContent.Create(new { PlayerId = winnerId, Quantity = 1, Price = price }),
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await _httpClient.SendAsync(message, cancellationToken);

        response.EnsureSuccessStatusCode();

        // NOTE: у публичного предмета продавца нет — ответ пустой; читаем как строку, чтобы не падать на пустом JSON.
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(body) ? null : JsonSerializer.Deserialize<Guid?>(body);
    }
}
