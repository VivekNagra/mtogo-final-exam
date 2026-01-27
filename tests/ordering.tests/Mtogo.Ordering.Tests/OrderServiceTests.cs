using Microsoft.Extensions.Logging;
using Moq;
using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;
using MassTransit;
using Mtogo.Contracts;

namespace Mtogo.Ordering.Tests;

public class OrderServiceTests
{
    private readonly Mock<ILegacyMenuClient> _legacyMock = new();
    private readonly Mock<IMenuItemPriceProvider> _priceMock = new();
    private readonly Mock<IOrderPricingRules> _pricingMock = new();
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();

    private OrderService CreateSvc() => new(
        _legacyMock.Object,
        _priceMock.Object,
        _pricingMock.Object,
        _loggerMock.Object,
        _publishMock.Object);

    [Fact]
    public async Task CreateOrderAsync_Should_Return_202_And_Publish_Event_When_Valid()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var req = new CreateOrderRequest(restaurantId, new List<CreateOrderItem> { new(itemId, 1) });

        _legacyMock.Setup(x => x.RestaurantExistsAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _priceMock.Setup(x => x.TryGetPrice(itemId, out It.Ref<decimal>.IsAny))
            .Callback(new TryGetPriceCallback((Guid id, out decimal p) => p = 10m))
            .Returns(true);


        _pricingMock.Setup(x => x.CalculateTotal(It.IsAny<IEnumerable<PricedOrderItem>>()))
            .Returns(new OrderPricingResult(10m, 0m, 0m, 10m));

        var svc = CreateSvc();

        // Act
        var (ok, status, _) = await svc.CreateOrderAsync(req, CancellationToken.None);

        // Assert
        Assert.True(ok);
        Assert.Equal(202, status);

        _publishMock.Verify(x => x.Publish(
            It.Is<OrderPlacedEvent>(e => e.RestaurantId == restaurantId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private delegate void TryGetPriceCallback(Guid id, out decimal price);
}