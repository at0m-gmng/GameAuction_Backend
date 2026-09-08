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

    public async Task DebitAsync(Guid playerId, decimal amount, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/auth/internal/players/{playerId}/debit",
            new { Amount = amount },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
