using GameBackend.Services.Generation.Domain;

namespace GameBackend.Services.Generation.Application.Interfaces;

/// <summary>
/// Контракт репозитория пулов генерации.
/// </summary>
public interface IGenerationPoolRepository
{
    /// <summary>
    /// Возвращает все архетипы предметов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <remarks>Только для чтения: сущности не отслеживаются EF, сохранять их через репозиторий нельзя.</remarks>
    Task<IReadOnlyCollection<ItemArchetype>> GetArchetypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает все уровни редкости.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <remarks>Только для чтения: сущности не отслеживаются EF, сохранять их через репозиторий нельзя.</remarks>
    Task<IReadOnlyCollection<RarityTier>> GetRarityTiersAsync(CancellationToken cancellationToken = default);
}