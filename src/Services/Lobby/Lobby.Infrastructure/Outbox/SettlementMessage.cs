using GameBackend.Services.Lobby.Application.Settlement;

namespace GameBackend.Services.Lobby.Infrastructure.Outbox;

/// <summary>
/// Запись outbox расчётов: состояние доставки расчёта одного лобби с повторами и экспоненциальной задержкой.
/// </summary>
public sealed class SettlementMessage
{
    /// <summary>
    /// Число попыток, после которого расчёт откладывается и требует ручного разбора.
    /// </summary>
    public const int MaxAttempts = 20;

    /// <summary>
    /// Максимальная длина сохраняемого текста ошибки.
    /// </summary>
    public const int LastErrorMaxLength = 2000;

    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Лобби аукциона (уникально: один расчёт на лобби).
    /// </summary>
    public Guid LobbyId { get; private set; }

    /// <summary>
    /// Разыгранный предмет.
    /// </summary>
    public Guid ItemId { get; private set; }

    /// <summary>
    /// Победитель.
    /// </summary>
    public Guid WinnerId { get; private set; }

    /// <summary>
    /// Выигрышная ставка.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Момент постановки в очередь (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Не раньше этого момента (UTC) выполняется следующая попытка.
    /// </summary>
    public DateTime NextAttemptAt { get; private set; }

    /// <summary>
    /// Момент успешного расчёта (UTC); null — ещё не проведён.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Число неудачных попыток.
    /// </summary>
    public int Attempts { get; private set; }

    /// <summary>
    /// Текст последней ошибки.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Исчерпаны ли попытки.
    /// </summary>
    public bool IsParked => ProcessedAt is null && Attempts >= MaxAttempts;

    private SettlementMessage()
    {
    }

    /// <summary>
    /// Создаёт запись для немедленной доставки.
    /// </summary>
    /// <param name="settlement">Расчёт по аукциону.</param>
    /// <param name="now">Текущий момент (UTC).</param>
    /// <returns>Новая запись outbox.</returns>
    public static SettlementMessage Create(AuctionSettlement settlement, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        LobbyId = settlement.LobbyId,
        ItemId = settlement.ItemId,
        WinnerId = settlement.WinnerId,
        Amount = settlement.Amount,
        CreatedAt = now,
        NextAttemptAt = now,
    };

    /// <summary>
    /// Восстанавливает расчёт из записи.
    /// </summary>
    /// <returns>Расчёт по аукциону.</returns>
    public AuctionSettlement ToSettlement() => new(LobbyId, ItemId, WinnerId, Amount);

    /// <summary>
    /// Отмечает расчёт проведённым.
    /// </summary>
    /// <param name="now">Текущий момент (UTC).</param>
    public void MarkProcessed(DateTime now)
    {
        ProcessedAt = now;
        LastError = null;
    }

    /// <summary>
    /// Фиксирует неудачную попытку и откладывает следующую: 15 с, 30 с, 1 мин … не дольше 10 мин.
    /// </summary>
    /// <param name="error">Текст ошибки.</param>
    /// <param name="now">Текущий момент (UTC).</param>
    public void MarkFailed(string error, DateTime now)
    {
        Attempts++;
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;

        double delaySeconds = Math.Min(BaseRetryDelay.TotalSeconds * Math.Pow(2, Attempts - 1), MaxRetryDelay.TotalSeconds);
        NextAttemptAt = now.AddSeconds(delaySeconds);
    }
}
