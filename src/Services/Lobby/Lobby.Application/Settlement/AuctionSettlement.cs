namespace GameBackend.Services.Lobby.Application.Settlement;

/// <summary>
/// Расчёт по завершённому аукциону: передать предмет победителю, списать с него ставку, начислить продавцу.
/// </summary>
/// <param name="LobbyId">Лобби аукциона; основа ключей идемпотентности.</param>
/// <param name="ItemId">Разыгранный предмет.</param>
/// <param name="WinnerId">Победитель.</param>
/// <param name="Amount">Выигрышная ставка.</param>
public sealed record AuctionSettlement(Guid LobbyId, Guid ItemId, Guid WinnerId, decimal Amount);
