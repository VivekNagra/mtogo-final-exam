using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Tests;

public sealed class OrderServiceTests
{
    private static OrderService CreateService(Mock<ILegacyMenuClient> legacy, IMenuItemPriceProvider prices)
    {
        var pricing = new Mock<IOrderPricingRules>();
        pricing.Setup(x => x.CalculateTotal(It.IsAny<IEnumerable<PricedOrderItem>>()))
            .Returns(new OrderPricingResult(0m, 0m, 0m, 0m));

        return new OrderService(legacy.Object, prices, pricing.Object, NullLogger<OrderService>.Instance);
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
    public async Task CreateOrder_Returns400_WhenRestaurantIdMissing()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        var svc = CreateService(legacy, new FakePriceProvider());

        var req = new CreateOrderRequest(Guid.Empty, new List<CreateOrderItem> { new(Guid.NewGuid(), 1) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(400, result.statusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns400_WhenItemsEmpty()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        var svc = CreateService(legacy, new FakePriceProvider());

        var req = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItem>());
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(400, result.statusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns400_WhenRestaurantUnknown()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        legacy.Setup(x => x.RestaurantExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(false);

        var svc = CreateService(legacy, new FakePriceProvider());

        var req = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItem> { new(Guid.NewGuid(), 1) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(400, result.statusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns202_WhenValid()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        legacy.Setup(x => x.RestaurantExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(true);

        var svc = CreateService(legacy, new FakePriceProvider());

        var req = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItem> { new(Guid.NewGuid(), 1) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.True(result.ok);
        Assert.Equal(202, result.statusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns400_WhenQuantityNotPositive()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        var svc = CreateService(legacy, new FakePriceProvider());

        var req = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItem> { new(Guid.NewGuid(), 0) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(400, result.statusCode);
    }

}
