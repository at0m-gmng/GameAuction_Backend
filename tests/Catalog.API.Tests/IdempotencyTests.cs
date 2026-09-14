using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Tests;

public class IdempotencyTests
{
    private static (CatalogDbContext Context, SqliteConnection Connection) NewDatabase()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<CatalogDbContext>().UseSqlite(connection).Options;
        var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
    }

    [Fact]
    public async Task GrantItem_SameKeyTwice_CreatesSingleItem()
    {
        var (context, connection) = NewDatabase();
        using var _ = connection;
        using var __ = context;

        var handler = new GrantItemCommandHandler(new ItemRepository(context), new InventoryRepository(context), context);
        var playerId = Guid.NewGuid();
        var command = new GrantItemCommand($"welcome-gift:{playerId}", playerId, "Rifle", "desc", ItemCategory.Weapons, ItemRarity.Common, "img", 100m);

        await handler.Handle(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, await context.Items.CountAsync());
        Assert.Equal(1, await context.InventoryItems.CountAsync());
        Assert.Equal(1, await context.ProcessedOperations.CountAsync());
    }

    [Fact]
    public async Task AwardItem_SameKeyTwice_AwardsOnceAndReturnsSameSeller()
    {
        var (context, connection) = NewDatabase();
        using var _ = connection;
        using var __ = context;

        var sellerId = Guid.NewGuid();
        var item = Item.CreateOwned(sellerId, "Rifle", "desc", ItemCategory.Weapons, ItemRarity.Rare, "img", 100m);
        item.ListForSale(100m);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var handler = new AwardItemCommandHandler(new ItemRepository(context), new InventoryRepository(context), context);
        var winnerId = Guid.NewGuid();
        var command = new AwardItemCommand($"award:{Guid.NewGuid()}", winnerId, item.Id, 1, 200m);

        var firstSeller = await handler.Handle(command, CancellationToken.None);
        var replaySeller = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(sellerId, firstSeller);
        Assert.Equal(sellerId, replaySeller);
        Assert.Equal(1, await context.InventoryItems.CountAsync(x => x.PlayerId == winnerId && x.ItemId == item.Id));
        Assert.Equal(1, await context.ProcessedOperations.CountAsync());

        var reloaded = await context.Items.FirstAsync(x => x.Id == item.Id);
        Assert.Equal(winnerId, reloaded.OwnerId);
    }
}
