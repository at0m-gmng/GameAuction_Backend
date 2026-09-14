using GameBackend.Services.Catalog.API.Domain;

namespace Catalog.API.Tests;

public class InventoryItemTests
{
    [Fact]
    public void Create_StartsWithSingleCopy()
    {
        var playerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var inventory = InventoryItem.Create(playerId, itemId);

        Assert.Equal(playerId, inventory.PlayerId);
        Assert.Equal(itemId, inventory.ItemId);
        Assert.Equal(1, inventory.Quantity);
    }

    [Fact]
    public void AddQuantity_IncreasesQuantity()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid());

        inventory.AddQuantity(3);

        Assert.Equal(4, inventory.Quantity);
    }

    [Fact]
    public void RemoveQuantity_DecreasesQuantity()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid());
        inventory.AddQuantity(2);

        inventory.RemoveQuantity(1);

        Assert.Equal(2, inventory.Quantity);
    }

    [Fact]
    public void RemoveQuantity_MoreThanAvailable_Throws()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => inventory.RemoveQuantity(2));
    }
}
