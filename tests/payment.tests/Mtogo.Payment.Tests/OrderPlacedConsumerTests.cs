using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Mtogo.Contracts;
using Mtogo.Payment.Api.Consumers;

namespace Mtogo.Payment.Tests;

public sealed class OrderPlacedConsumerTests
{
    [Fact]
    public async Task Consume_PublishesPaymentFailed_WhenTotalPriceAbove500()
    {
        var logger = Mock.Of<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger);

        var orderId = Guid.NewGuid();
        var message = new OrderPlacedEvent(orderId, Guid.NewGuid(), 501m, DateTime.UtcNow);

        var context = new Mock<ConsumeContext<OrderPlacedEvent>>();
        context.SetupGet(x => x.Message).Returns(message);

        context
            .Setup(x => x.Publish(It.IsAny<PaymentFailedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await consumer.Consume(context.Object);

        context.Verify(
            x => x.Publish(It.Is<PaymentFailedEvent>(e => e.OrderId == orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_DoesNotPublishPaymentFailed_WhenTotalPriceAtOrBelow500()
    {
        var logger = Mock.Of<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger);

        var message = new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid(), 500m, DateTime.UtcNow);

        var context = new Mock<ConsumeContext<OrderPlacedEvent>>();
        context.SetupGet(x => x.Message).Returns(message);

        await consumer.Consume(context.Object);

        context.Verify(
            x => x.Publish(It.IsAny<PaymentFailedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

