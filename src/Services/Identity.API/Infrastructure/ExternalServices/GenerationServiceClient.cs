using System.Net.Http.Json;
using System.Text.Json;

namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-реализация клиента к Generation.API. BaseAddress и заголовок
/// X-Internal-Key настраиваются при регистрации HttpClient в Program.cs.
/// </summary>
public sealed class GenerationServiceClient : IGenerationServiceClient
{
    // NOTE: ASP.NET Core сериализует ответ в camelCase, а ReadFromJsonAsync (в
    // отличие от серверного [FromBody]-биндинга) по умолчанию регистрозависим —
    // без этой опции все поля молча придут пустыми.
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
