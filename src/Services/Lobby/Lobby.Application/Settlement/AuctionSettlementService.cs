using GameBackend.Services.Lobby.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.Application.Settlement;

/// <summary>
/// Проводит расчёт по аукциону межсервисными вызовами. Все шаги идемпотентны по ключам лобби — повтор безопасен.
/// </summary>
public sealed class AuctionSettlementService
{
    private const string AwardKeyPrefix = "award:";
    private const string DebitKeyPrefix = "debit:";
    private const string CreditKeyPrefix = "credit:";

    private readonly IIdentityServiceClient _identityClient;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly IPlayerNotifier _notifier;
    private readonly ILogger<AuctionSettlementService> _logger;

    /// <summary>
    /// Инициализирует сервис клиентами сервисов и уведомителем.
    /// </summary>
    /// <param name="identityClient">Клиент к Identity.API (списание/начисление).</param>
    /// <param name="catalogClient">Клиент к Catalog.API (передача предмета).</param>
    /// <param name="notifier">Адресные уведомления об изменении баланса.</param>
    /// <param name="logger">Логгер.</param>
    public AuctionSettlementService(
        IIdentityServiceClient identityClient,
        ICatalogServiceClient catalogClient,
        IPlayerNotifier notifier,
        ILogger<AuctionSettlementService> logger)
    {
        _identityClient = identityClient;
        _catalogClient = catalogClient;
        _notifier = notifier;
        _logger = logger;
    }

    /// <summary>
    /// Передаёт предмет победителю, списывает ставку и начисляет её продавцу (если лот был игроковым).
    /// Любая ошибка пробрасывается — outbox повторит расчёт целиком.
    /// </summary>
    /// <param name="settlement">Расчёт по аукциону.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SettleAsync(AuctionSettlement settlement, CancellationToken cancellationToken)
    {
        Guid? sellerId = await _catalogClient.AwardItemAsync(
            settlement.ItemId, settlement.WinnerId, settlement.Amount, AwardKeyPrefix + settlement.LobbyId, cancellationToken);

        await _identityClient.DebitAsync(settlement.WinnerId, settlement.Amount, DebitKeyPrefix + settlement.LobbyId, cancellationToken);
        await _notifier.NotifyBalanceChangedAsync(settlement.WinnerId, cancellationToken);

        if (sellerId.HasValue)
        {
            await _identityClient.CreditAsync(sellerId.Value, settlement.Amount, CreditKeyPrefix + settlement.LobbyId, cancellationToken);
            await _notifier.NotifyBalanceChangedAsync(sellerId.Value, cancellationToken);
            _logger.LogInformation("Продавец {SellerId} получил {Amount} за предмет {ItemId} в лобби {LobbyId}",
                sellerId.Value, settlement.Amount, settlement.ItemId, settlement.LobbyId);
        }
    }
}
