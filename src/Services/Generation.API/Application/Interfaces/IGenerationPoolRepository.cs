using GameBackend.Services.Generation.API.Domain;

namespace GameBackend.Services.Generation.API.Application.Interfaces;

/// <summary>
/// Контракт репозитория пулов генерации.
/// </summary>
public interface IGenerationPoolRepository
{
    /// <summary>
    /// Возвращает все архетипы предметов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyCollection<ItemArchetype>> GetArchetypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает все уровни редкости.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyCollection<RarityTier>> GetRarityTiersAsync(CancellationToken cancellationToken = default);
}