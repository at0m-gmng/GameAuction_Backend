using System.Net.Http.Json;
using System.Text.Json;

namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-клиент к Generation.API; BaseAddress и X-Internal-Key настраиваются в Program.cs.
/// </summary>
public sealed class GenerationServiceClient : IGenerationServiceClient
{
    // NOTE: ReadFromJsonAsync регистрозависим по умолчанию — без опции camelCase-поля придут пустыми.
    private static readonly JsonSerializerOptions ResponseOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public GenerationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeneratedItemResponse> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("api/generation/generate", content: null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeneratedItemResponse>(ResponseOptions, cancellationToken);
        return result ?? throw new HttpRequestException("Generation.API вернул пустой ответ.");
    }
}
