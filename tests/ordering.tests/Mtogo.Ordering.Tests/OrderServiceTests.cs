using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_Returns400_WhenRestaurantIdMissing()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        var svc = new OrderService(legacy.Object, NullLogger<OrderService>.Instance);

        var req = new CreateOrderRequest(Guid.Empty, new List<CreateOrderItem> { new(Guid.NewGuid(), 1) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(400, result.statusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns400_WhenItemsEmpty()
    {
        var legacy = new Mock<ILegacyMenuClient>();
        var svc = new OrderService(legacy.Object, NullLogger<OrderService>.Instance);

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

        var svc = new OrderService(legacy.Object, NullLogger<OrderService>.Instance);

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

        var svc = new OrderService(legacy.Object, NullLogger<OrderService>.Instance);

        var req = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItem> { new(Guid.NewGuid(), 1) });
        var result = await svc.CreateOrderAsync(req, CancellationToken.None);

        Assert.True(result.ok);
        Assert.Equal(202, result.statusCode);
    }
}
