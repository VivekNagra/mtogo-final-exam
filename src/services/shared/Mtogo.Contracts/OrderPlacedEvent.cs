namespace Mtogo.Contracts;

public record OrderPlacedEvent(
    Guid OrderId,
    Guid RestaurantId,
    decimal TotalPrice,
    DateTime OccurredOn);