namespace GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;

/// <summary>
/// Клиент к Generation.API.
/// </summary>
public interface IGenerationServiceClient
{
    /// <summary>
    /// Запрашивает генерацию одной заготовки предмета.
    /// </summary>
    /// <exception cref="HttpRequestException">Если Generation.API недоступен или вернул ошибку.</exception>
    Task<GeneratedItemResponse> GenerateAsync(CancellationToken cancellationToken = default);
}
