using GameBackend.Services.Catalog.API.Domain;
using GameBackend.SharedKernel.Domain;

namespace Catalog.API.Tests;

public class ItemTests
{
    private static Item CreatePublic(int stock = 5) =>
        Item.Create("Plasma Rifle", "desc", ItemCategory.Weapons, ItemRarity.Common, "img", 100m, stock);

    private static Item CreateOwned(Guid ownerId) =>
        Item.CreateOwned(ownerId, "Plasma Rifle", "desc", ItemCategory.Weapons, ItemRarity.Rare, "img", 100m);

    [Fact]
    public void Create_PublicItem_IsListedWithoutOwner()
    {
        var item = CreatePublic();

        Assert.True(item.IsListed);
        Assert.Null(item.OwnerId);
    }

    [Fact]
    public void CreateOwned_PrivateItem_NotListedZeroStockWithOwner()
    {
        var ownerId = Guid.NewGuid();

        var item = CreateOwned(ownerId);

        Assert.False(item.IsListed);
        Assert.Equal(0, item.Stock);
        Assert.Equal(ownerId, item.OwnerId);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Item.Create("", "desc", ItemCategory.Weapons, ItemRarity.Common, "img", 100m, 1));
    }

    [Fact]
    public void Create_NegativeStartingPrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Item.Create("Name", "desc", ItemCategory.Weapons, ItemRarity.Common, "img", -1m, 1));
    }

    [Fact]
    public void Create_NegativeStock_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Item.Create("Name", "desc", ItemCategory.Weapons, ItemRarity.Common, "img", 100m, -1));
    }

    [Fact]
    public void ListForSale_SetsPriceAndListed()
    {
        var item = CreateOwned(Guid.NewGuid());

        item.ListForSale(250m);

        Assert.True(item.IsListed);
        Assert.Equal(250m, item.StartingPrice);
    }

    [Fact]
    public void ListForSale_NonPositivePrice_Throws()
    {
        var item = CreateOwned(Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => item.ListForSale(0m));
    }

    [Fact]
    public void Unlist_ClearsListed()
    {
        var item = CreatePublic();

        item.Unlist();

        Assert.False(item.IsListed);
    }

    [Fact]
    public void TransferTo_ChangesOwnerToWinner()
    {
        var sellerId = Guid.NewGuid();
        var winnerId = Guid.NewGuid();
        var item = CreateOwned(sellerId);

        item.TransferTo(winnerId);

        Assert.Equal(winnerId, item.OwnerId);
    }

    [Fact]
    public void DecreaseStock_ReducesStock()
    {
        var item = CreatePublic(5);

        item.DecreaseStock(2);

        Assert.Equal(3, item.Stock);
    }

    [Fact]
    public void DecreaseStock_MoreThanAvailable_Throws()
    {
        var item = CreatePublic(1);

        Assert.Throws<InvalidOperationException>(() => item.DecreaseStock(2));
    }

    [Fact]
    public void DecreaseStock_NonPositiveQuantity_Throws()
    {
        var item = CreatePublic(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.DecreaseStock(0));
    }
}
