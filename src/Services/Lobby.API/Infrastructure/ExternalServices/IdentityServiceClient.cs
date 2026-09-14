using System.Net.Http.Json;

namespace GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-клиент к Identity.API; BaseAddress и X-Internal-Key настраиваются в Program.cs.
/// </summary>
public sealed class IdentityServiceClient : IIdentityServiceClient
{
    private readonly HttpClient _httpClient;

    public IdentityServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DebitAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var response = await SendWithKeyAsync($"api/auth/internal/players/{playerId}/debit", amount, idempotencyKey, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreditAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var response = await SendWithKeyAsync($"api/auth/internal/players/{playerId}/credit", amount, idempotencyKey, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendWithKeyAsync(string path, decimal amount, string idempotencyKey, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { Amount = amount }),
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);

        return await _httpClient.SendAsync(message, cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/auth/internal/players/{playerId}/balance", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<decimal>(cancellationToken: cancellationToken);
    }
}
