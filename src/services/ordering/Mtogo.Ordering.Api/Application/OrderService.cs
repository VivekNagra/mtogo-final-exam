using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;
using Mtogo.Contracts;
using MassTransit;

namespace Mtogo.Ordering.Api.Application;

public sealed class OrderService
{
    private readonly ILegacyMenuClient _legacy;
    private readonly IMenuItemPriceProvider _prices;
    private readonly IOrderPricingRules _pricing;
    private readonly ILogger<OrderService> _log;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(
        ILegacyMenuClient legacy,
        IMenuItemPriceProvider prices,
        IOrderPricingRules pricing,
        ILogger<OrderService> log,
        IPublishEndpoint publishEndpoint)
    {
        _legacy = legacy;
        _prices = prices;
        _pricing = pricing;
        _log = log;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<(bool ok, int statusCode, object body)> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {
        if (req is null)
            return (false, 400, new { message = "request body required" });

        if (req.RestaurantId == Guid.Empty)
            return (false, 400, new { message = "restaurantId required" });

        if (req.Items is null || req.Items.Count == 0)
            return (false, 400, new { message = "items required" });

        if (req.Items.Any(i => i.Quantity <= 0))
            return (false, 400, new { message = "quantity must be > 0" });

        bool exists;
        try
        {
            exists = await _legacy.RestaurantExistsAsync(req.RestaurantId, ct);
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(ex, "Legacy menu unavailable when validating RestaurantId {RestaurantId}", req.RestaurantId);
            return (false, 503, new { message = "Legacy menu unavailable" });
        }
        catch (TaskCanceledException ex)
        {
            _log.LogWarning(ex, "Legacy menu timeout when validating RestaurantId {RestaurantId}", req.RestaurantId);
            return (false, 503, new { message = "Legacy menu timeout" });
        }

        if (!exists)
            return (false, 400, new { message = "Unknown restaurant (legacy menu)" });

        var pricedItems = new List<PricedOrderItem>(req.Items.Count);
        foreach (var item in req.Items)
        {
            if (!_prices.TryGetPrice(item.MenuItemId, out var unitPrice))
                return (false, 400, new { message = $"Unknown menu item price for {item.MenuItemId}" });

            pricedItems.Add(new PricedOrderItem(item.MenuItemId, item.Quantity, unitPrice));
        }

        var pricing = _pricing.CalculateTotal(pricedItems);
        var orderId = Guid.NewGuid();

        // INTEGRATION: Publish the event to RabbitMQ
        // This is the trigger for the Saga pattern
        await _publishEndpoint.Publish(new OrderPlacedEvent(
            orderId,
            req.RestaurantId,
            pricing.Total,
            DateTime.UtcNow), ct);

        _log.LogInformation(
            "Order created {OrderId} for Restaurant {RestaurantId} and event published",
            orderId,
            req.RestaurantId);

        return (true, 202, new { orderId, status = "Accepted", totalPrice = pricing.Total });
    }
}

public sealed record CreateOrderRequest(Guid RestaurantId, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid MenuItemId, int Quantity);