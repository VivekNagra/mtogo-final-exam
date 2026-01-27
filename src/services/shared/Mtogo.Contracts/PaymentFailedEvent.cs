namespace Mtogo.Contracts;

public record PaymentFailedEvent(
    Guid OrderId, 
    string Reason, 
    DateTime OccurredOn);