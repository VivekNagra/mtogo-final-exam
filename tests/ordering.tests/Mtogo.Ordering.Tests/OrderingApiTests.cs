using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using Moq;
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
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Mock.Of<IPublishEndpoint>());

                // Replace legacy client with fake for integration tests
                services.AddSingleton<ILegacyMenuClient>(new FakeLegacyMenuClient(true));
                services.AddSingleton<IMenuItemPriceProvider>(new FakePriceProvider());
                services.AddSingleton<IOrderPricingRules, OrderPricingRules>();
            });
        });
    }

    private WebApplicationFactory<Program> CreateFactory(
        bool restaurantExists = true,
        IMenuItemPriceProvider? priceProvider = null,
        IOrderPricingRules? pricingRules = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPublishEndpoint>();
                services.AddSingleton(Mock.Of<IPublishEndpoint>());

                services.RemoveAll<ILegacyMenuClient>();
                services.RemoveAll<IMenuItemPriceProvider>();
                if (pricingRules is not null)
                    services.RemoveAll<IOrderPricingRules>();

                services.AddSingleton<ILegacyMenuClient>(new FakeLegacyMenuClient(restaurantExists));
                services.AddSingleton<IMenuItemPriceProvider>(priceProvider ?? new FakePriceProvider());
                if (pricingRules is not null)
                    services.AddSingleton(pricingRules);
            });
        });
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

    private sealed class FixedPriceProvider : IMenuItemPriceProvider
    {
        private readonly decimal _price;

        public FixedPriceProvider(decimal price) => _price = price;

        public bool TryGetPrice(Guid menuItemId, out decimal price)
        {
            price = _price;
            return true;
        }
    }

    private sealed class InMemoryPriceProviderAdapter : IMenuItemPriceProvider
    {
        private readonly InMemoryMenuItemPriceProvider _inner = new();

        public bool TryGetPrice(Guid menuItemId, out decimal price)
            => _inner.TryGetPrice(menuItemId, out price);
    }

    [Fact]
    public async Task POST_orders_ReturnsTotalPrice_WhenValid()
    {
        var client = CreateFactory(priceProvider: new FixedPriceProvider(100.00m)).CreateClient();

        var req = new
        {
            restaurantId = Guid.NewGuid(),
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 2 } }
        };

        var resp = await client.PostAsJsonAsync("/api/orders", req);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var total = doc.RootElement.GetProperty("totalPrice").GetDecimal();
        Assert.Equal(200.00m, total);
    }

    [Fact]
    public async Task POST_orders_AppliesDeliveryFee_WhenSubtotalBelowThreshold()
    {
        var client = CreateFactory(
            priceProvider: new FixedPriceProvider(90.00m),
            pricingRules: new OrderPricingRules()).CreateClient();

        var req = new
        {
            restaurantId = Guid.NewGuid(),
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 2 } } // subtotal 180
        };

        var resp = await client.PostAsJsonAsync("/api/orders", req);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var total = doc.RootElement.GetProperty("totalPrice").GetDecimal();
        Assert.Equal(209.00m, total);
    }

    [Fact]
    public async Task POST_orders_UsesInMemoryPrices_ForSeededItems()
    {
        var client = CreateFactory(priceProvider: new InMemoryPriceProviderAdapter()).CreateClient();

        var req = new
        {
            restaurantId = Guid.NewGuid(),
            items = new[]
            {
                new { menuItemId = Guid.Parse("22222222-2222-2222-2222-222222222222"), quantity = 1 },
                new { menuItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"), quantity = 1 }
            }
        };

        var resp = await client.PostAsJsonAsync("/api/orders", req);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var total = doc.RootElement.GetProperty("totalPrice").GetDecimal();
        Assert.Equal(137.00m, total);
    }

}
