using System.Net.Http.Json;

namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-клиент к Lobby.API; BaseAddress и X-Internal-Key настраиваются в Program.cs.
/// </summary>
public sealed class LobbyServiceClient : ILobbyServiceClient
{
    private sealed record CreateLobbyRequest(Guid ItemId, string ItemName, string? ItemImageUrl, decimal StartingPrice, int MaxParticipants);

    private readonly HttpClient _httpClient;

    public LobbyServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid> CreateLobbyAsync(
        Guid itemId,
        string itemName,
        string? itemImageUrl,
        decimal startingPrice,
        int maxParticipants,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateLobbyRequest(itemId, itemName, itemImageUrl, startingPrice, maxParticipants);
        var response = await _httpClient.PostAsJsonAsync("api/lobbies", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
    }
}
