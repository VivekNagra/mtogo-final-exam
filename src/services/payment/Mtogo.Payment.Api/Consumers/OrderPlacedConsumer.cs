using MassTransit;
using Mtogo.Contracts;

namespace Mtogo.Payment.Api.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;

        // Demo Scenario: simulating a payment failure for large orders
        if (message.TotalPrice > 500)
        {
            _logger.LogWarning("SAGA STEP: Payment REJECTED for Order {Id} due to high amount", message.OrderId);
            
            await context.Publish(new PaymentFailedEvent(
                message.OrderId, 
                "Insufficient funds / Credit limit exceeded", 
                DateTime.UtcNow));
            return;
        }

        _logger.LogInformation("SAGA STEP: Payment APPROVED for Order {Id}", message.OrderId);
    }
}