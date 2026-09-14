using GameBackend.Services.Identity.API.Domain.ValueObjects;

namespace Identity.API.Tests;

public class GoldCreditsTests
{
    [Fact]
    public void Constructor_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoldCredits(-1));
    }

    [Fact]
    public void Constructor_ValidAmount_SetsAmount()
    {
        var credits = new GoldCredits(150m);

        Assert.Equal(150m, credits.Amount);
    }

    [Fact]
    public void Zero_HasZeroAmount()
    {
        Assert.Equal(0m, GoldCredits.Zero.Amount);
    }

    [Fact]
    public void Add_IncreasesAmount()
    {
        var result = new GoldCredits(100m).Add(new GoldCredits(50m));

        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Spend_SufficientFunds_DecreasesAmount()
    {
        var result = new GoldCredits(100m).Spend(new GoldCredits(30m));

        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Spend_InsufficientFunds_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new GoldCredits(10m).Spend(new GoldCredits(11m)));
    }

    [Fact]
    public void Equality_SameAmount_AreEqual()
    {
        Assert.Equal(new GoldCredits(100m), new GoldCredits(100m));
        Assert.NotEqual(new GoldCredits(100m), new GoldCredits(101m));
    }
}
