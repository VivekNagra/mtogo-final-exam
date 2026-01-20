using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Api.Application;

public sealed class OrderService
{
    private readonly ILegacyMenuClient _legacy;
    private readonly IMenuItemPriceProvider _prices;
    private readonly IOrderPricingRules _pricing;
    private readonly ILogger<OrderService> _log;

    public OrderService(
        ILegacyMenuClient legacy,
        IMenuItemPriceProvider prices,
        IOrderPricingRules pricing,
        ILogger<OrderService> log)
    {
        _legacy = legacy;
        _prices = prices;
        _pricing = pricing;
        _log = log;
    }

    public async Task<(bool ok, int statusCode, object body)> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {
        // Defensive guard (helps tests and prevents null reference if binding fails)
        if (req is null)
            return (false, 400, new { message = "request body required" });

        // Validation (also used for SQ test cases / taint mitigation)
        if (req.RestaurantId == Guid.Empty)
            return (false, 400, new { message = "restaurantId required" });

        if (req.Items is null || req.Items.Count == 0)
            return (false, 400, new { message = "items required" });

        if (req.Items.Any(i => i.Quantity <= 0))
            return (false, 400, new { message = "quantity must be > 0" });

        // Integration boundary: untrusted input flows into downstream HTTP call.
        // If legacy is unavailable/timeout -> map to 503 to avoid leaking infrastructure exceptions to clients.
        bool exists;
        try
        {
            exists = await _legacy.RestaurantExistsAsync(req.RestaurantId, ct);
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(
                ex,
                "Legacy menu unavailable when validating RestaurantId {RestaurantId}",
                req.RestaurantId);

            return (false, 503, new { message = "Legacy menu unavailable" });
        }
        catch (TaskCanceledException ex)
        {
            _log.LogWarning(
                ex,
                "Legacy menu timeout when validating RestaurantId {RestaurantId}",
                req.RestaurantId);

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

        // Safe logging (no full request body / no PII)
        _log.LogInformation(
            "Order created {OrderId} for Restaurant {RestaurantId}",
            orderId,
            req.RestaurantId);

        return (true, 202, new { orderId, status = "Created", totalPrice = pricing.Total });
    }
}

public sealed record CreateOrderRequest(Guid RestaurantId, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid MenuItemId, int Quantity);
