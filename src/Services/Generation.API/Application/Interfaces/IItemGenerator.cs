namespace GameBackend.Services.Generation.API.Domain;

/// <summary>
/// Контракт генератора заготовок предметов.
/// </summary>
public interface IItemGenerator
{
    /// <summary>
    /// Собирает заготовку из случайного архетипа и уровня редкости.
    /// </summary>
    /// <param name="archetypes">Пулы архетипов.</param>
    /// <param name="tiers">Уровни редкости.</param>
    GeneratedItemDto Create(IReadOnlyCollection<ItemArchetype> archetypes, IReadOnlyCollection<RarityTier> tiers);
}