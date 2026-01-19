using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Api.Application;

public sealed class OrderService
{
  private readonly ILegacyMenuClient _legacy;
  private readonly ILogger<OrderService> _log;

  public OrderService(ILegacyMenuClient legacy, ILogger<OrderService> log)
  {
    _legacy = legacy;
    _log = log;
  }

  public async Task<(bool ok, int statusCode, object body)> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
  {
    // Validation (also used for SQ security test cases / taint mitigation)
    if (req.RestaurantId == Guid.Empty)
      return (false, 400, new { message = "restaurantId required" });

    if (req.Items is null || req.Items.Count == 0)
      return (false, 400, new { message = "items required" });

    if (req.Items.Any(i => i.Quantity <= 0))
      return (false, 400, new { message = "quantity must be > 0" });

    // Integration with legacy (this is also a taint sink boundary: untrusted input flows into downstream HTTP call)
    var exists = await _legacy.RestaurantExistsAsync(req.RestaurantId, ct);
    if (!exists)
      return (false, 400, new { message = "Unknown restaurant (legacy menu)" });

    var orderId = Guid.NewGuid();

    // Safe logging (no full request body)
    _log.LogInformation("Order created {OrderId} for Restaurant {RestaurantId}", orderId, req.RestaurantId);

    return (true, 202, new { orderId, status = "Created" });
  }
}

// Keep DTOs here for now (or move to Contracts later)
public sealed record CreateOrderRequest(Guid RestaurantId, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid MenuItemId, int Quantity);
