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

    public Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;
        
        
        _logger.LogInformation("SAGA STEP: Processing payment for Order {OrderId}. Amount: {Amount}", 
            message.OrderId, message.TotalPrice);

        
        return Task.CompletedTask;
    }
}