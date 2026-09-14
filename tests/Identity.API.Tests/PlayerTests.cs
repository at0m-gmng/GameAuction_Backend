using GameBackend.Services.Identity.API.Domain;
using GameBackend.Services.Identity.API.Domain.ValueObjects;

namespace Identity.API.Tests;

public class PlayerTests
{
    private static Player RegisterPlayer(decimal startingBalance = 1000m) =>
        Player.Register("Neo", "Neo@Nexus.io", "hash", new GoldCredits(startingBalance));

    [Fact]
    public void Register_ValidData_SetsFieldsAndStartingBalance()
    {
        var player = RegisterPlayer(500m);

        Assert.Equal("Neo", player.Nickname);
        Assert.Equal("Neo@Nexus.io", player.Email);
        Assert.Equal(500m, player.Balance.Amount);
        Assert.True(player.StartingBalanceGranted);
    }

    [Fact]
    public void Register_NormalizesEmailToLowercase()
    {
        var player = RegisterPlayer();

        Assert.Equal("neo@nexus.io", player.NormalizedEmail);
    }

    [Theory]
    [InlineData("", "e@x.io", "hash")]
    [InlineData("Neo", "", "hash")]
    [InlineData("Neo", "e@x.io", "")]
    public void Register_EmptyField_Throws(string nickname, string email, string passwordHash)
    {
        Assert.Throws<ArgumentException>(() =>
            Player.Register(nickname, email, passwordHash, GoldCredits.Zero));
    }

    [Fact]
    public void AddToBalance_IncreasesBalance()
    {
        var player = RegisterPlayer(100m);

        player.AddToBalance(new GoldCredits(250m));

        Assert.Equal(350m, player.Balance.Amount);
    }

    [Fact]
    public void SpendFromBalance_SufficientFunds_DecreasesBalance()
    {
        var player = RegisterPlayer(100m);

        player.SpendFromBalance(new GoldCredits(40m));

        Assert.Equal(60m, player.Balance.Amount);
    }

    [Fact]
    public void SpendFromBalance_InsufficientFunds_Throws()
    {
        var player = RegisterPlayer(30m);

        Assert.Throws<InvalidOperationException>(() => player.SpendFromBalance(new GoldCredits(31m)));
    }

    [Fact]
    public void GrantStartingBalanceIfNeeded_AlreadyGranted_DoesNothing()
    {
        var player = RegisterPlayer(1000m);

        var granted = player.GrantStartingBalanceIfNeeded(new GoldCredits(5000m));

        Assert.False(granted);
        Assert.Equal(1000m, player.Balance.Amount);
    }
}
