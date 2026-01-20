using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Tests;

public sealed class OrderingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrderingApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureServices(services =>
            {
                // Replace legacy client with deterministic fake for integration tests
                services.AddSingleton<ILegacyMenuClient>(new FakeLegacyMenuClient(true));
                services.AddSingleton<IMenuItemPriceProvider>(new FakePriceProvider());
                services.AddSingleton<IOrderPricingRules, OrderPricingRules>();
            });
        });
    }

    [Fact]
    public async Task POST_orders_Returns202_WhenValid()
    {
        var client = _factory.CreateClient();

        var req = new
        {
            restaurantId = Guid.NewGuid(),
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 1 } }
        };

        var resp = await client.PostAsJsonAsync("/api/orders", req);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);
    }

    private sealed class FakeLegacyMenuClient : ILegacyMenuClient
    {
        private readonly bool _exists;
        public FakeLegacyMenuClient(bool exists) => _exists = exists;
        public Task<bool> RestaurantExistsAsync(Guid restaurantId, CancellationToken ct) => Task.FromResult(_exists);
    }

    private sealed class FakePriceProvider : IMenuItemPriceProvider
    {
        public bool TryGetPrice(Guid menuItemId, out decimal price)
        {
            price = 10.00m;
            return true;
        }
    }

    [Fact]
    public async Task POST_orders_Returns400_WhenItemsMissing()
    {
        var client = _factory.CreateClient();

        var req = new { restaurantId = Guid.NewGuid() }; // no items
        var resp = await client.PostAsJsonAsync("/api/orders", req);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task POST_orders_Returns400_WhenQuantityInvalid()
    {
        var client = _factory.CreateClient();

        var req = new
        {
            restaurantId = Guid.NewGuid(),
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 0 } }
        };

        var resp = await client.PostAsJsonAsync("/api/orders", req);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

}
