using GameBackend.SharedKernel.Application;
using GameBackend.Services.Catalog.API.Application.Items;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Запрос предметов, которые вызывающий игрок выставил на продажу.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-владельца (из JWT).</param>
public sealed record GetMyListingsQuery(Guid PlayerId) : IQuery<IReadOnlyCollection<ItemCatalogDto>>;
