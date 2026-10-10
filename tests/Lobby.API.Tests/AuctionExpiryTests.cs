using GameBackend.Services.Lobby.Application.Services;
using GameBackend.Services.Lobby.Domain;
using GameBackend.Services.Lobby.Infrastructure.Persistence;
using GameBackend.Services.Lobby.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Lobby.API.Tests;

public sealed class AuctionExpiryTests : IDisposable
{
    private static readonly TimeSpan ShortAuction = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan WaitPastEnd = TimeSpan.FromMilliseconds(400);

    private readonly SqliteConnection _connection = new("Filename=:memory:");
    private readonly DbContextOptions<LobbyDbContext> _options;

    public AuctionExpiryTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<LobbyDbContext>().UseSqlite(_connection).Options;
        using LobbyDbContext context = new(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private async Task<LobbyAggregate> SeedBiddingLobbyAsync(TimeSpan duration, bool withBid)
    {
        Guid player = Guid.NewGuid();
        LobbyAggregate lobby = LobbyAggregate.Create(Guid.NewGuid(), "Rifle", null, ItemRarity.Common, 100m, 4);
        lobby.Join(player);
        lobby.StartAuction(duration);
        if (withBid)
        {
            lobby.PlaceBid(player, 150m, 1000m, TimeSpan.Zero);
        }

        using LobbyDbContext context = new(_options);
        context.Lobbies.Add(lobby);
        await context.SaveChangesAsync();
        return lobby;
    }

    [Fact]
    public async Task GetExpiredBiddingLobbyIds_ReturnsOnlyBiddingLobbiesPastEnd()
    {
        LobbyAggregate expired = await SeedBiddingLobbyAsync(ShortAuction, withBid: true);
        LobbyAggregate running = await SeedBiddingLobbyAsync(TimeSpan.FromHours(1), withBid: true);
        await Task.Delay(WaitPastEnd);

        using LobbyDbContext context = new(_options);
        IReadOnlyCollection<Guid> ids = await new LobbyRepository(context, new NoopDomainEventDispatcher())
            .GetExpiredBiddingLobbyIdsAsync(DateTime.UtcNow);

        Assert.Contains(expired.Id, ids);
        Assert.DoesNotContain(running.Id, ids);
    }

    [Fact]
    public async Task CompleteIfExpiredById_WithBid_CompletesAndEnqueuesSettlement()
    {
        LobbyAggregate seeded = await SeedBiddingLobbyAsync(ShortAuction, withBid: true);
        await Task.Delay(WaitPastEnd);
        FakeSettlementOutbox outbox = new();

        using (LobbyDbContext context = new(_options))
        {
            await new AuctionCompletionService(new LobbyRepository(context, new NoopDomainEventDispatcher()), outbox)
                .CompleteIfExpiredAsync(seeded.Id, CancellationToken.None);
        }

        using LobbyDbContext check = new(_options);
        LobbyAggregate stored = await check.Lobbies.SingleAsync(x => x.Id == seeded.Id);
        Assert.Equal(LobbyStatus.Completed, stored.Status);
        Assert.Single(outbox.Enqueued);
    }

    [Fact]
    public async Task CompleteIfExpiredById_NotYetExpired_LeavesLobbyUntouched()
    {
        LobbyAggregate seeded = await SeedBiddingLobbyAsync(TimeSpan.FromHours(1), withBid: true);
        FakeSettlementOutbox outbox = new();

        using (LobbyDbContext context = new(_options))
        {
            await new AuctionCompletionService(new LobbyRepository(context, new NoopDomainEventDispatcher()), outbox)
                .CompleteIfExpiredAsync(seeded.Id, CancellationToken.None);
        }

        using LobbyDbContext check = new(_options);
        Assert.Equal(LobbyStatus.Bidding, (await check.Lobbies.SingleAsync(x => x.Id == seeded.Id)).Status);
        Assert.Empty(outbox.Enqueued);
    }
}
