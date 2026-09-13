using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-клиент к Identity.API; BaseAddress и X-Internal-Key настраиваются в Program.cs.
/// </summary>
public sealed class IdentityServiceClient : IIdentityServiceClient
{
    private sealed record ErrorResponse([property: JsonPropertyName("message")] string? Message);

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

        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
            throw new InvalidOperationException(error?.Message ?? "Недостаточно средств");
        }

        throw new InvalidOperationException("Сервис оплаты недоступен — попробуйте ещё раз");
    }
}
