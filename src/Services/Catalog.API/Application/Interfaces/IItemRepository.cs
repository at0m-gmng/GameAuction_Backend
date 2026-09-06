using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с каталожными карточками предметов.
/// Определяет контракт для сохранения, загрузки и поиска предметов.
/// Реализация находится в Infrastructure слое.
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Получает предмет по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор предмета.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Предмет или null, если не найден.</returns>
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает предметы по списку идентификаторов.
    /// </summary>
    /// <param name="ids">Идентификаторы предметов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция найденных предметов.</returns>
    Task<IReadOnlyCollection<Item>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает список предметов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="category">Категория предмета. Null означает все категории.</param>
    /// <param name="minimumRarity">Минимальная редкость предмета. Null означает любую редкость.</param>
    /// <param name="skip">Количество элементов, которые нужно пропустить.</param>
    /// <param name="take">Количество элементов, которые нужно получить.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Коллекция предметов, подходящих под фильтр.</returns>
    Task<IReadOnlyCollection<Item>> GetListAsync(
        ItemCategory? category,
        ItemRarity? minimumRarity,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет предмет в хранилище.
    /// </summary>
    /// <param name="item">Предмет для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task SaveAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет предмет из хранилища.
    /// </summary>
    /// <param name="item">Предмет для удаления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);
}