using GameBackend.SharedKernel.Application;
using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Application.Items;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Обработчик запроса списка предметов каталога: загружает из репозитория и преобразует в DTO для UI.
/// </summary>
public sealed class GetItemsQueryHandler : IQueryHandler<GetItemsQuery, IReadOnlyCollection<ItemCatalogDto>>
{
    /// <summary>
    /// Максимально допустимый размер страницы.
    /// </summary>
    private const int MaxPageSize = 100;

    /// <summary>
    /// Репозиторий предметов.
    /// </summary>
    private readonly IItemRepository _repository;

    /// <summary>
    /// Инициализирует обработчик запроса списка предметов.
    /// </summary>
    /// <param name="repository">Репозиторий предметов.</param>
    public GetItemsQueryHandler(IItemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Обрабатывает запрос списка предметов.
    /// </summary>
    /// <param name="query">Запрос списка предметов.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Коллекция предметов для отображения в каталоге.</returns>
    public async Task<IReadOnlyCollection<ItemCatalogDto>> Handle(
        GetItemsQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var skip = (page - 1) * pageSize;

        var items = await _repository.GetListAsync(
            query.Category,
            query.MinimumRarity,
            skip,
            pageSize,
            cancellationToken);

        return items
            .Select(item => new ItemCatalogDto(
                item.Id,
                item.Name,
                item.Description,
                item.Category,
                item.Rarity,
                item.ImageUrl,
                item.StartingPrice,
                item.Stock))
            .ToArray();
    }
}