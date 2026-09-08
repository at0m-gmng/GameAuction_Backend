using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда выставления предмета из инвентаря на аукцион.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-владельца (из JWT).</param>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="StartingPrice">Стартовая цена аукциона.</param>
public sealed record ListInventoryItemForAuctionCommand(Guid PlayerId, Guid ItemId, decimal StartingPrice) : ICommand<Guid>;
