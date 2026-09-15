using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Services;
using GameBackend.Services.Lobby.API.Domain;
using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;
using GameBackend.SharedKernel.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lobby.API.Tests;

public class AuctionSettlementTests
{
    private sealed class FakeCatalogServiceClient : ICatalogServiceClient
    {
        private readonly Guid? _seller;
        public Guid AwardedItemId { get; private set; }
        public Guid AwardedWinnerId { get; private set; }
        public decimal AwardedPrice { get; private set; }
        public string? AwardKey { get; private set; }

        public FakeCatalogServiceClient(Guid? seller) => _seller = seller;

        public Task<Guid?> AwardItemAsync(Guid itemId, Guid winnerId, decimal price, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            AwardedItemId = itemId;
            AwardedWinnerId = winnerId;
            AwardedPrice = price;
            AwardKey = idempotencyKey;
            return Task.FromResult(_seller);
        }
    }

    private sealed class FakeIdentityServiceClient : IIdentityServiceClient
    {
        public List<(Guid playerId, decimal amount, string key)> Debits { get; } = new();
        public List<(Guid playerId, decimal amount, string key)> Credits { get; } = new();

        public Task DebitAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            Debits.Add((playerId, amount, idempotencyKey));
            return Task.CompletedTask;
        }

        public Task CreditAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            Credits.Add((playerId, amount, idempotencyKey));
            return Task.CompletedTask;
        }

        public Task<decimal> GetBalanceAsync(Guid playerId, CancellationToken cancellationToken = default) => Task.FromResult(1000m);
    }

    private sealed class FakeLobbyRepository : ILobbyRepository
    {
        public Task<LobbyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<LobbyAggregate?> GetOpenLobbyByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<LobbyAggregate>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<LobbyAggregate>> GetRecentCompletedLobbiesAsync(int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<LobbyAggregate>> GetPlayerHistoryAsync(Guid playerId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(int Wins, int Losses)> GetPlayerAuctionStatsAsync(Guid playerId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SaveAsync(LobbyAggregate lobby, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private static IHubContext<LobbyHub> NoopHub()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSignalR();
        return services.BuildServiceProvider().GetRequiredService<IHubContext<LobbyHub>>();
    }

    private static LobbyAggregate BiddingLobbyWithWinner(Guid itemId, Guid winner, decimal bid)
    {
        var lobby = LobbyAggregate.Create(itemId, "Rifle", null, ItemRarity.Common, 100m, 4);
        lobby.Join(winner);
        lobby.StartAuction(TimeSpan.FromMinutes(5));
        lobby.PlaceBid(winner, bid, 1000m, TimeSpan.FromSeconds(15));
        return lobby;
    }

    [Fact]
    public async Task CompleteAsync_PlayerListedItem_DebitsWinnerAndCreditsSeller()
    {
        var itemId = Guid.NewGuid();
        var winner = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var lobby = BiddingLobbyWithWinner(itemId, winner, 200m);

        var catalog = new FakeCatalogServiceClient(sellerId);
        var identity = new FakeIdentityServiceClient();
        var service = new AuctionCompletionService(new FakeLobbyRepository(), identity, catalog, NoopHub(), NullLogger<AuctionCompletionService>.Instance);

        await service.CompleteAsync(lobby, CancellationToken.None);

        Assert.Equal(LobbyStatus.Completed, lobby.Status);
        Assert.Equal(winner, lobby.WinnerId);

        Assert.Equal(itemId, catalog.AwardedItemId);
        Assert.Equal(winner, catalog.AwardedWinnerId);
        Assert.Equal(200m, catalog.AwardedPrice);
        Assert.Equal($"award:{lobby.Id}", catalog.AwardKey);

        Assert.Contains(identity.Debits, d => d.playerId == winner && d.amount == 200m && d.key == $"debit:{lobby.Id}");
        Assert.Contains(identity.Credits, c => c.playerId == sellerId && c.amount == 200m && c.key == $"credit:{lobby.Id}");
    }

    [Fact]
    public async Task CompleteAsync_PublicItem_DebitsWinnerWithoutCredit()
    {
        var winner = Guid.NewGuid();
        var lobby = BiddingLobbyWithWinner(Guid.NewGuid(), winner, 150m);

        var catalog = new FakeCatalogServiceClient(seller: null);
        var identity = new FakeIdentityServiceClient();
        var service = new AuctionCompletionService(new FakeLobbyRepository(), identity, catalog, NoopHub(), NullLogger<AuctionCompletionService>.Instance);

        await service.CompleteAsync(lobby, CancellationToken.None);

        Assert.Contains(identity.Debits, d => d.playerId == winner && d.amount == 150m);
        Assert.Empty(identity.Credits);
    }
}
