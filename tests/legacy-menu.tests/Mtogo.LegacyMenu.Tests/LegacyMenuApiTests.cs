using System.Net;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mtogo.LegacyMenu.Api.Data;
using Mtogo.LegacyMenu.Api.Models;
using Xunit;

namespace Mtogo.LegacyMenu.Tests;

public sealed class LegacyMenuApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LegacyMenuApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove ALL existing DbContext registrations (Npgsql + options)
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<LegacyMenuDbContext>)
                             || d.ServiceType == typeof(LegacyMenuDbContext))
                    .ToList();

                foreach (var d in dbContextDescriptors)
                    services.Remove(d);

                // Register one provider only: InMemory
                services.AddDbContext<LegacyMenuDbContext>(opts =>
                    opts.UseInMemoryDatabase("legacy-menu-api-tests"));
            });
        });

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LegacyMenuDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Restaurants.Add(new Restaurant { Id = SeedIds.RestaurantId, Name = "Test" });
        db.MenuItems.AddRange(
            new MenuItem { Id = SeedIds.BurgerId, RestaurantId = SeedIds.RestaurantId, Name = "Burger", Price = 10m },
            new MenuItem { Id = SeedIds.FriesId, RestaurantId = SeedIds.RestaurantId, Name = "Fries", Price = 5m }
        );

        db.SaveChanges();
    }

    [Fact]
    public async Task GET_menu_ReturnsItems()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/legacy/menu/{SeedIds.RestaurantId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var items = await resp.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GET_menu_Returns404_WhenRestaurantMissing()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/legacy/menu/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GET_menuItem_ReturnsItem_WhenPresent()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/legacy/menu/item/{SeedIds.BurgerId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var item = await resp.Content.ReadFromJsonAsync<MenuItemDetailsDto>();
        Assert.NotNull(item);
        Assert.Equal(SeedIds.BurgerId, item.Id);
        Assert.Equal(SeedIds.RestaurantId, item.RestaurantId);
    }

    [Fact]
    public async Task GET_menuItem_Returns404_WhenMissing()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/legacy/menu/item/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    private sealed record MenuItemDto(Guid Id, string Name, decimal Price);
    private sealed record MenuItemDetailsDto(Guid Id, Guid RestaurantId, string Name, decimal Price);
}
