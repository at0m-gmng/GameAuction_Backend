using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Infrastructure.Outbox;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lobby.API.Tests;

public sealed class SettlementOutboxTests : IDisposable
{
    private static readonly TimeSpan SignalTimeout = TimeSpan.FromSeconds(1);

    private readonly SqliteConnection _connection = new("Filename=:memory:");
    private readonly LobbyDbContext _context;
    private readonly FixedTimeProvider _time = new();

    public SettlementOutboxTests()
    {
        _connection.Open();
        _context = new LobbyDbContext(new DbContextOptionsBuilder<LobbyDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private SettlementOutboxDispatcher Dispatcher(FakeIdentityServiceClient identity) =>
        new(_context,
            new AuctionSettlementService(identity, new FakeCatalogServiceClient(seller: null), new FakePlayerNotifier(), NullLogger<AuctionSettlementService>.Instance),
            _time,
            NullLogger<SettlementOutboxDispatcher>.Instance);

    private async Task<SettlementMessage> EnqueueAsync()
    {
        SettlementMessage message = SettlementMessage.Create(new AuctionSettlement(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m), _time.GetUtcNow().UtcDateTime);
        _context.SettlementMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    [Fact]
    public async Task Enqueue_SignalsProcessorAfterSave()
    {
        using SettlementOutboxSignal signal = new();
        SettlementOutbox outbox = new(_context, signal, _time);

        outbox.Enqueue(new AuctionSettlement(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m));
        await _context.SaveChangesAsync();

        Assert.True(await signal.WaitAsync(SignalTimeout, CancellationToken.None));
        Assert.Equal(1, await _context.SettlementMessages.CountAsync());
    }

    [Fact]
    public async Task Enqueue_DuplicateLobby_RejectedAndDetachedSoContextStaysUsable()
    {
        using SettlementOutboxSignal signal = new();
        SettlementOutbox outbox = new(_context, signal, _time);
        AuctionSettlement settlement = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);
        outbox.Enqueue(settlement);
        await _context.SaveChangesAsync();

        outbox.Enqueue(settlement with { });

        await Assert.ThrowsAsync<DuplicateEntryException>(() => _context.SaveChangesAsync());
        await _context.SaveChangesAsync();
        Assert.Equal(1, await _context.SettlementMessages.CountAsync());
    }

    [Fact]
    public async Task DispatchDueAsync_Success_MarksProcessed()
    {
        SettlementMessage message = await EnqueueAsync();
        FakeIdentityServiceClient identity = new();

        int taken = await Dispatcher(identity).DispatchDueAsync(CancellationToken.None);

        Assert.Equal(1, taken);
        Assert.NotNull(message.ProcessedAt);
        Assert.Single(identity.Debits);
    }

    [Fact]
    public async Task DispatchDueAsync_Failure_SchedulesRetryWithBackoff()
    {
        SettlementMessage message = await EnqueueAsync();
        FakeIdentityServiceClient identity = new() { FailDebit = true };

        await Dispatcher(identity).DispatchDueAsync(CancellationToken.None);

        Assert.Null(message.ProcessedAt);
        Assert.Equal(1, message.Attempts);
        Assert.Equal(_time.GetUtcNow().UtcDateTime.AddSeconds(15), message.NextAttemptAt);
        Assert.NotNull(message.LastError);
    }

    [Fact]
    public async Task DispatchDueAsync_NotYetDue_IsSkippedUntilBackoffElapses()
    {
        SettlementMessage message = await EnqueueAsync();
        FakeIdentityServiceClient identity = new() { FailDebit = true };
        await Dispatcher(identity).DispatchDueAsync(CancellationToken.None);
        identity.FailDebit = false;

        int takenEarly = await Dispatcher(identity).DispatchDueAsync(CancellationToken.None);
        _time.Now = _time.Now.AddSeconds(15);
        int takenLater = await Dispatcher(identity).DispatchDueAsync(CancellationToken.None);

        Assert.Equal(0, takenEarly);
        Assert.Equal(1, takenLater);
        Assert.NotNull(message.ProcessedAt);
    }

    [Fact]
    public void MarkFailed_CapsDelayAndParksAfterMaxAttempts()
    {
        DateTime now = _time.GetUtcNow().UtcDateTime;
        SettlementMessage message = SettlementMessage.Create(new AuctionSettlement(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m), now);

        for (int i = 0; i < SettlementMessage.MaxAttempts; i++)
        {
            message.MarkFailed("boom", now);
        }

        Assert.Equal(now.AddMinutes(10), message.NextAttemptAt);
        Assert.True(message.IsParked);
    }
}
