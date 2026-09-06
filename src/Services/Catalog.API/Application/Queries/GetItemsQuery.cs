using GameBackend.SharedKernel.Application;
using GameBackend.Services.Catalog.API.Application.Items;
using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Запрос списка предметов каталога с фильтрацией и пагинацией.
/// </summary>
/// <param name="Category">Категория предмета. Null означает все категории.</param>
/// <param name="MinimumRarity">Минимальная редкость предмета. Null означает любую редкость.</param>
/// <param name="Page">Номер страницы, начиная с 1.</param>
/// <param name="PageSize">Размер страницы.</param>
public sealed record GetItemsQuery(
    ItemCategory? Category,
    ItemRarity? MinimumRarity,
    int Page,
    int PageSize) : IQuery<IReadOnlyCollection<ItemCatalogDto>>;