using GameBackend.Services.Lobby.Application.Settlement;

namespace GameBackend.Services.Lobby.Application.Interfaces;

/// <summary>
/// Outbox расчётов: расчёт пишется в БД вместе с завершением лобби и доставляется фоновым обработчиком с повторами.
/// </summary>
public interface ISettlementOutbox
{
    /// <summary>
    /// Ставит расчёт в очередь; запись сохраняется в той же транзакции, что и следующее сохранение лобби.
    /// </summary>
    /// <param name="settlement">Расчёт по аукциону.</param>
    void Enqueue(AuctionSettlement settlement);
}
