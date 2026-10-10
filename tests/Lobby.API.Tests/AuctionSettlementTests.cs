using GameBackend.Services.Lobby.Application.Services;
using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Domain;
using GameBackend.SharedKernel.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lobby.API.Tests;

public class AuctionSettlementTests
{
    private static LobbyAggregate BiddingLobbyWithWinner(Guid itemId, Guid winner, decimal bid)
    {
        LobbyAggregate lobby = LobbyAggregate.Create(itemId, "Rifle", null, ItemRarity.Common, 100m, 4);
        lobby.Join(winner);
        lobby.StartAuction(TimeSpan.FromMinutes(5));
        lobby.PlaceBid(winner, bid, 1000m, TimeSpan.FromSeconds(15));
        return lobby;
    }

    private static AuctionSettlementService SettlementService(FakeIdentityServiceClient identity, FakeCatalogServiceClient catalog, FakePlayerNotifier notifier) =>
        new(identity, catalog, notifier, NullLogger<AuctionSettlementService>.Instance);

    [Fact]
    public async Task CompleteAsync_WithWinner_CompletesLobbyAndEnqueuesSettlement()
    {
        Guid itemId = Guid.NewGuid();
        Guid winner = Guid.NewGuid();
        LobbyAggregate lobby = BiddingLobbyWithWinner(itemId, winner, 200m);
        FakeSettlementOutbox outbox = new();
        AuctionCompletionService service = new(new FakeLobbyRepository(), outbox);

        await service.CompleteAsync(lobby, CancellationToken.None);

        Assert.Equal(LobbyStatus.Completed, lobby.Status);
        AuctionSettlement settlement = Assert.Single(outbox.Enqueued);
        Assert.Equal(new AuctionSettlement(lobby.Id, itemId, winner, 200m), settlement);
    }

    [Fact]
    public async Task SettleAsync_PlayerListedItem_DebitsWinnerAndCreditsSeller()
    {
        Guid itemId = Guid.NewGuid();
        Guid winner = Guid.NewGuid();
        Guid sellerId = Guid.NewGuid();
        Guid lobbyId = Guid.NewGuid();
        FakeCatalogServiceClient catalog = new(sellerId);
        FakeIdentityServiceClient identity = new();
        FakePlayerNotifier notifier = new();

        await SettlementService(identity, catalog, notifier).SettleAsync(new AuctionSettlement(lobbyId, itemId, winner, 200m), CancellationToken.None);

        Assert.Equal(itemId, catalog.AwardedItemId);
        Assert.Equal(winner, catalog.AwardedWinnerId);
        Assert.Equal(200m, catalog.AwardedPrice);
        Assert.Equal($"award:{lobbyId}", catalog.AwardKey);
        Assert.Contains(identity.Debits, d => d.playerId == winner && d.amount == 200m && d.key == $"debit:{lobbyId}");
        Assert.Contains(identity.Credits, c => c.playerId == sellerId && c.amount == 200m && c.key == $"credit:{lobbyId}");
        Assert.Equal(new[] { winner, sellerId }, notifier.Notified);
    }

    [Fact]
    public async Task SettleAsync_PublicItem_DebitsWinnerWithoutCredit()
    {
        Guid winner = Guid.NewGuid();
        FakeIdentityServiceClient identity = new();

        await SettlementService(identity, new FakeCatalogServiceClient(seller: null), new FakePlayerNotifier())
            .SettleAsync(new AuctionSettlement(Guid.NewGuid(), Guid.NewGuid(), winner, 150m), CancellationToken.None);

        Assert.Contains(identity.Debits, d => d.playerId == winner && d.amount == 150m);
        Assert.Empty(identity.Credits);
    }
}
