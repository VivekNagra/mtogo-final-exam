using MassTransit;
using Mtogo.Contracts;

namespace Mtogo.Ordering.Api.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(ILogger<PaymentFailedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        // In your oral exam, explain that this is where you'd update 
        // the database status to 'Cancelled' or 'Failed'.
        _logger.LogCritical("SAGA COMPENSATION: Order {Id} is now CANCELLED. Reason: {Reason}",
            context.Message.OrderId,
            context.Message.Reason);

        return Task.CompletedTask;
    }
}