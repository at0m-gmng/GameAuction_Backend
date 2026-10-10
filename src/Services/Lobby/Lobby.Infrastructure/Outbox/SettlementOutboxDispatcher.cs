using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.Infrastructure.Outbox;

/// <summary>
/// Доставляет созревшие расчёты из outbox: успех — отметка, ошибка — повтор с экспоненциальной задержкой.
/// </summary>
public sealed class SettlementOutboxDispatcher
{
    /// <summary>
    /// Сколько расчётов обрабатывается за один проход.
    /// </summary>
    public const int BatchSize = 20;

    private readonly LobbyDbContext _context;
    private readonly AuctionSettlementService _settlement;
    private readonly TimeProvider _time;
    private readonly ILogger<SettlementOutboxDispatcher> _logger;

    /// <summary>
    /// Инициализирует диспетчер зависимостями.
    /// </summary>
    /// <param name="context">Контекст БД лобби.</param>
    /// <param name="settlement">Сервис расчёта.</param>
    /// <param name="time">Источник времени.</param>
    /// <param name="logger">Логгер.</param>
    public SettlementOutboxDispatcher(
        LobbyDbContext context,
        AuctionSettlementService settlement,
        TimeProvider time,
        ILogger<SettlementOutboxDispatcher> logger)
    {
        _context = context;
        _settlement = settlement;
        _time = time;
        _logger = logger;
    }

    /// <summary>
    /// Проводит до <see cref="BatchSize"/> созревших расчётов; каждый результат сохраняется сразу.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сколько расчётов было взято в работу.</returns>
    public async Task<int> DispatchDueAsync(CancellationToken cancellationToken)
    {
        DateTime now = _time.GetUtcNow().UtcDateTime;

        List<SettlementMessage> due = await _context.SettlementMessages
            .Where(x => x.ProcessedAt == null && x.Attempts < SettlementMessage.MaxAttempts && x.NextAttemptAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (SettlementMessage message in due)
        {
            try
            {
                await _settlement.SettleAsync(message.ToSettlement(), cancellationToken);
                message.MarkProcessed(_time.GetUtcNow().UtcDateTime);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                message.MarkFailed(ex.Message, _time.GetUtcNow().UtcDateTime);

                if (message.IsParked)
                {
                    _logger.LogError(ex, "Расчёт по лобби {LobbyId} отложен после {Attempts} попыток — нужен ручной разбор",
                        message.LobbyId, message.Attempts);
                }
                else
                {
                    _logger.LogWarning(ex, "Расчёт по лобби {LobbyId} не удался (попытка {Attempts}), повтор в {NextAttemptAt}",
                        message.LobbyId, message.Attempts, message.NextAttemptAt);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return due.Count;
    }
}
