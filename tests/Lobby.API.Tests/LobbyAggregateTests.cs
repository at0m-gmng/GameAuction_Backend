using GameBackend.Services.Lobby.API.Domain;
using GameBackend.SharedKernel.Domain;

namespace Lobby.API.Tests;

public class LobbyAggregateTests
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(15);

    private static LobbyAggregate CreateLobby(int maxParticipants = 4) =>
        LobbyAggregate.Create(Guid.NewGuid(), "Rifle", "img", ItemRarity.Common, 100m, maxParticipants);

    private static LobbyAggregate BiddingLobbyWith(params Guid[] players)
    {
        var lobby = CreateLobby();
        foreach (var player in players)
            lobby.Join(player);
        lobby.StartAuction(TimeSpan.FromMinutes(5));
        return lobby;
    }

    [Fact]
    public void Create_StartsInGathering()
    {
        var lobby = CreateLobby();

        Assert.Equal(LobbyStatus.Gathering, lobby.Status);
        Assert.Null(lobby.CurrentBid);
        Assert.Null(lobby.EndsAt);
        Assert.Null(lobby.WinnerId);
    }

    [Fact]
    public void Create_NonPositiveMaxParticipants_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LobbyAggregate.Create(Guid.NewGuid(), "Rifle", "img", ItemRarity.Common, 100m, 0));
    }

    [Fact]
    public void Join_NewPlayer_ReturnsTrueAndAdds()
    {
        var lobby = CreateLobby();
        var player = Guid.NewGuid();

        var isNew = lobby.Join(player);

        Assert.True(isNew);
        Assert.Contains(player, lobby.Participants);
    }

    [Fact]
    public void Join_SamePlayerTwice_SecondReturnsFalse()
    {
        var lobby = CreateLobby();
        var player = Guid.NewGuid();

        lobby.Join(player);

        Assert.False(lobby.Join(player));
    }

    [Fact]
    public void Join_BeyondCapacity_Throws()
    {
        var lobby = CreateLobby(maxParticipants: 1);
        lobby.Join(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => lobby.Join(Guid.NewGuid()));
    }

    [Fact]
    public void StartAuction_FromGathering_SetsBiddingAndDeadline()
    {
        var lobby = CreateLobby();
        lobby.Join(Guid.NewGuid());

        lobby.StartAuction(TimeSpan.FromMinutes(5));

        Assert.Equal(LobbyStatus.Bidding, lobby.Status);
        Assert.NotNull(lobby.EndsAt);
    }

    [Fact]
    public void StartAuction_NoParticipants_Throws()
    {
        var lobby = CreateLobby();

        Assert.Throws<InvalidOperationException>(() => lobby.StartAuction(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void PlaceBid_FirstBidBelowStartingPrice_Throws()
    {
        var player = Guid.NewGuid();
        var lobby = BiddingLobbyWith(player);

        Assert.Throws<InvalidOperationException>(() => lobby.PlaceBid(player, 50m, 1000m, Window));
    }

    [Fact]
    public void PlaceBid_FirstValidBid_IsRecorded()
    {
        var player = Guid.NewGuid();
        var lobby = BiddingLobbyWith(player);

        lobby.PlaceBid(player, 100m, 1000m, Window);

        Assert.NotNull(lobby.CurrentBid);
        Assert.Equal(100m, lobby.CurrentBid!.Amount);
        Assert.Equal(player, lobby.CurrentBid.PlayerId);
    }

    [Fact]
    public void PlaceBid_NotHigherThanCurrent_Throws()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var lobby = BiddingLobbyWith(first, second);
        lobby.PlaceBid(first, 200m, 1000m, Window);

        Assert.Throws<InvalidOperationException>(() => lobby.PlaceBid(second, 200m, 1000m, Window));
    }

    [Fact]
    public void PlaceBid_HigherThanCurrent_UpdatesCurrentBid()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var lobby = BiddingLobbyWith(first, second);
        lobby.PlaceBid(first, 200m, 1000m, Window);

        lobby.PlaceBid(second, 300m, 1000m, Window);

        Assert.Equal(300m, lobby.CurrentBid!.Amount);
        Assert.Equal(second, lobby.CurrentBid.PlayerId);
    }

    [Fact]
    public void PlaceBid_ExceedsBalance_Throws()
    {
        var player = Guid.NewGuid();
        var lobby = BiddingLobbyWith(player);

        Assert.Throws<InvalidOperationException>(() => lobby.PlaceBid(player, 500m, 100m, Window));
    }

    [Fact]
    public void PlaceBid_NonParticipant_Throws()
    {
        var lobby = BiddingLobbyWith(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => lobby.PlaceBid(Guid.NewGuid(), 100m, 1000m, Window));
    }

    [Fact]
    public void PlaceBid_WhenNotBidding_Throws()
    {
        var player = Guid.NewGuid();
        var lobby = CreateLobby();
        lobby.Join(player);

        Assert.Throws<InvalidOperationException>(() => lobby.PlaceBid(player, 100m, 1000m, Window));
    }

    [Fact]
    public void Complete_WithBids_SetsWinnerToHighestBidder()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var lobby = BiddingLobbyWith(first, second);
        lobby.PlaceBid(first, 100m, 1000m, Window);
        lobby.PlaceBid(second, 200m, 1000m, Window);

        lobby.Complete();

        Assert.Equal(LobbyStatus.Completed, lobby.Status);
        Assert.Equal(second, lobby.WinnerId);
    }

    [Fact]
    public void Complete_WithoutBids_WinnerIsNull()
    {
        var lobby = BiddingLobbyWith(Guid.NewGuid());

        lobby.Complete();

        Assert.Equal(LobbyStatus.Completed, lobby.Status);
        Assert.Null(lobby.WinnerId);
    }

    [Fact]
    public void Complete_WhenNotBidding_Throws()
    {
        var lobby = CreateLobby();

        Assert.Throws<InvalidOperationException>(() => lobby.Complete());
    }

    [Fact]
    public void ExpireWithoutBids_NoBids_ResetsToGathering()
    {
        var lobby = BiddingLobbyWith(Guid.NewGuid());

        lobby.ExpireWithoutBids();

        Assert.Equal(LobbyStatus.Gathering, lobby.Status);
        Assert.Null(lobby.EndsAt);
        Assert.Empty(lobby.Participants);
    }

    [Fact]
    public void ExpireWithoutBids_WithBids_Throws()
    {
        var player = Guid.NewGuid();
        var lobby = BiddingLobbyWith(player);
        lobby.PlaceBid(player, 100m, 1000m, Window);

        Assert.Throws<InvalidOperationException>(() => lobby.ExpireWithoutBids());
    }
}
