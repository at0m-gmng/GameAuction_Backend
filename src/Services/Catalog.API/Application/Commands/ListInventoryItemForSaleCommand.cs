using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда выставления предмета из инвентаря на продажу — предмет попадает в каталог как лот.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-владельца (из JWT).</param>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="StartingPrice">Стартовая цена продажи.</param>
public sealed record ListInventoryItemForSaleCommand(Guid PlayerId, Guid ItemId, decimal StartingPrice) : ICommand;
