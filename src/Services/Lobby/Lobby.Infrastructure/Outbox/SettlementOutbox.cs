using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Lobby.Infrastructure.Outbox;

/// <summary>
/// EF-реализация outbox: запись попадает в тот же SaveChanges, что и лобби; после фиксации будится обработчик,
/// после неудачного сохранения запись отсоединяется, чтобы не мешать следующим сохранениям контекста.
/// </summary>
public sealed class SettlementOutbox : ISettlementOutbox
{
    private readonly LobbyDbContext _context;
    private readonly SettlementOutboxSignal _signal;
    private readonly TimeProvider _time;
    private readonly List<SettlementMessage> _pending = new();

    /// <summary>
    /// Инициализирует outbox контекстом БД лобби.
    /// </summary>
    /// <param name="context">Контекст БД лобби.</param>
    /// <param name="signal">Сигнал обработчику.</param>
    /// <param name="time">Источник времени.</param>
    public SettlementOutbox(LobbyDbContext context, SettlementOutboxSignal signal, TimeProvider time)
    {
        _context = context;
        _signal = signal;
        _time = time;
    }

    /// <inheritdoc />
    public void Enqueue(AuctionSettlement settlement)
    {
        SettlementMessage message = SettlementMessage.Create(settlement, _time.GetUtcNow().UtcDateTime);
        _context.SettlementMessages.Add(message);

        if (_pending.Count == 0)
        {
            _context.SavedChanges += OnSavedChanges;
            _context.SaveChangesFailed += OnSaveChangesFailed;
        }

        _pending.Add(message);
    }

    private void OnSavedChanges(object? sender, SavedChangesEventArgs e)
    {
        Unsubscribe();
        _signal.Notify();
    }

    private void OnSaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
    {
        foreach (SettlementMessage message in _pending)
        {
            _context.Entry(message).State = EntityState.Detached;
        }

        Unsubscribe();
    }

    private void Unsubscribe()
    {
        _context.SavedChanges -= OnSavedChanges;
        _context.SaveChangesFailed -= OnSaveChangesFailed;
        _pending.Clear();
    }
}
