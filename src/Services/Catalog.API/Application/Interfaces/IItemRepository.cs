using GameBackend.Services.Catalog.API.Domain;

namespace GameBackend.Services.Catalog.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с аукционными лотами.
/// Определяет контракт для сохранения и загрузки предметов.
/// Реализация будет находиться в Infrastructure слое.
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Получает аукционный лот по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор лота.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Аукционный лот или null, если не найден.</returns>
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет аукционный лот в хранилище.
    /// </summary>
    /// <param name="item">Аукционный лот для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task SaveAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет аукционный лот из хранилища.
    /// </summary>
    /// <param name="item">Аукционный лот для удаления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);
}