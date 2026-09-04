using GameBackend.Services.Catalog.API.Application.Items;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Queries;

/// <summary>
/// Запрос инвентаря игрока. Возвращает предметы с их количеством.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока (берётся из JWT).</param>
public sealed record GetInventoryQuery(Guid PlayerId) : IQuery<IReadOnlyCollection<InventoryItemDto>>;