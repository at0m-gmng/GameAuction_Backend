using GameBackend.SharedKernel.Application;
using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Application.Items;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Обработчик запроса собственных выставленных лотов игрока.
/// </summary>
public sealed class GetMyListingsQueryHandler : IQueryHandler<GetMyListingsQuery, IReadOnlyCollection<ItemCatalogDto>>
{
    private readonly IItemRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий предметов каталога.</param>
    public GetMyListingsQueryHandler(IItemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает предметы, выставленные вызывающим игроком.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyCollection<ItemCatalogDto>> Handle(GetMyListingsQuery query, CancellationToken cancellationToken)
    {
        var items = await _repository.GetListedByOwnerAsync(query.PlayerId, cancellationToken);

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
