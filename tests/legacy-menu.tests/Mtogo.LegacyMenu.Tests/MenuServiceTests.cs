using Microsoft.EntityFrameworkCore;
using Mtogo.LegacyMenu.Api.Data;
using Mtogo.LegacyMenu.Api.Models;
using Mtogo.LegacyMenu.Api.Repositories;
using Mtogo.LegacyMenu.Api.Services;

namespace Mtogo.LegacyMenu.Tests;

public sealed class MenuServiceTests
{
    private static LegacyMenuDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<LegacyMenuDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

        return new LegacyMenuDbContext(opts);
    }

    [Fact]
    public async Task RestaurantExists_ReturnsTrue_WhenRestaurantPresent()
    {
        await using var db = CreateDb();
        db.Restaurants.Add(new Restaurant { Id = SeedIds.RestaurantId, Name = "Test" });
        await db.SaveChangesAsync();

        var repo = new MenuRepository(db);
        var svc = new MenuService(repo);

        var exists = await svc.RestaurantExists(SeedIds.RestaurantId, CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task GetMenu_ReturnsItems_ForRestaurant()
    {
        await using var db = CreateDb();
        db.Restaurants.Add(new Restaurant { Id = SeedIds.RestaurantId, Name = "Test" });
        db.MenuItems.AddRange(
          new MenuItem { Id = SeedIds.BurgerId, RestaurantId = SeedIds.RestaurantId, Name = "Burger", Price = 10m },
          new MenuItem { Id = SeedIds.FriesId, RestaurantId = SeedIds.RestaurantId, Name = "Fries", Price = 5m }
        );
        await db.SaveChangesAsync();

        var repo = new MenuRepository(db);
        var svc = new MenuService(repo);

        var items = await svc.GetMenu(SeedIds.RestaurantId, CancellationToken.None);

        Assert.Equal(2, items.Count);
    }
}
