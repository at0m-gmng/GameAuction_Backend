using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Application.Lobbies;
using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Domain;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Domain;

namespace Lobby.API.Tests;

internal sealed class FakeCatalogServiceClient : ICatalogServiceClient
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

internal sealed class FakeIdentityServiceClient : IIdentityServiceClient
{
    public List<(Guid playerId, decimal amount, string key)> Debits { get; } = new();
    public List<(Guid playerId, decimal amount, string key)> Credits { get; } = new();

    public bool FailDebit { get; set; }

    public Task DebitAsync(Guid playerId, decimal amount, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (FailDebit)
        {
            throw new HttpRequestException("Identity.API недоступен");
        }

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

internal sealed class FakeLobbyRepository : ILobbyRepository
{
    public Task<LobbyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<LobbyAggregate?> GetOpenLobbyByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<IReadOnlyCollection<LobbyAggregate>> GetOpenLobbiesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<IReadOnlyCollection<Guid>> GetExpiredBiddingLobbyIdsAsync(DateTime now, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<IReadOnlyCollection<LobbyListDto>> GetRecentCompletedSummariesAsync(int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<IReadOnlyCollection<LobbyAggregate>> GetPlayerHistoryAsync(Guid playerId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<(int Wins, int Losses)> GetPlayerAuctionStatsAsync(Guid playerId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task SaveAsync(LobbyAggregate lobby, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakePlayerNotifier : IPlayerNotifier
{
    public List<Guid> Notified { get; } = new();

    public Task NotifyBalanceChangedAsync(Guid playerId, CancellationToken cancellationToken)
    {
        Notified.Add(playerId);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSettlementOutbox : ISettlementOutbox
{
    public List<AuctionSettlement> Enqueued { get; } = new();

    public void Enqueue(AuctionSettlement settlement) => Enqueued.Add(settlement);
}

internal sealed class FixedTimeProvider : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;
}

internal sealed class NoopDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
