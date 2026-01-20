using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mtogo.LegacyMenu.Api.Data;

namespace Mtogo.LegacyMenu.Tests;

public sealed class LegacyMenuDbInitializerTests
{
    private static LegacyMenuDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LegacyMenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LegacyMenuDbContext(options);
    }

    [Fact]
    public async Task InitializeAsync_Seeds_WhenDatabaseEmpty()
    {
        await using var db = CreateDb();
        var initializer = new LegacyMenuDbInitializer(db, NullLogger<LegacyMenuDbInitializer>.Instance);

        await initializer.InitializeAsync(CancellationToken.None);

        Assert.Equal(1, await db.Restaurants.CountAsync());
        Assert.Equal(3, await db.MenuItems.CountAsync());
    }

    [Fact]
    public async Task InitializeAsync_DoesNotSeed_WhenRestaurantsExist()
    {
        await using var db = CreateDb();
        db.Restaurants.Add(new() { Id = SeedIds.RestaurantId, Name = "Existing" });
        await db.SaveChangesAsync();

        var initializer = new LegacyMenuDbInitializer(db, NullLogger<LegacyMenuDbInitializer>.Instance);

        await initializer.InitializeAsync(CancellationToken.None);

        Assert.Equal(1, await db.Restaurants.CountAsync());
        Assert.Empty(await db.MenuItems.ToListAsync());
    }
}
