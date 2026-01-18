using System.Net;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("LegacyMenu", client =>
{
  var baseUrl = builder.Configuration["LegacyMenu:BaseUrl"] ?? "http://localhost:8081";
  client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ordering" }));

// TAINT SOURCE: user-controlled request body
// TAINT RELEVANCE: restaurantId/items/quantities are untrusted input
// TAINT SINKS (future): SQL persistence, logs, message publishing, downstream HTTP calls.
// MITIGATIONS: validation, parameterized SQL, safe logging, and CodeQL (taint/dataflow) evidence in CI.
app.MapPost("/api/orders", async (CreateOrderRequest req, IHttpClientFactory httpFactory, ILoggerFactory lf, CancellationToken ct) =>
{
  var log = lf.CreateLogger("Ordering");

  if (req.RestaurantId == Guid.Empty)
    return Results.BadRequest(new { message = "restaurantId required" });

  if (req.Items is null || req.Items.Count == 0)
    return Results.BadRequest(new { message = "items required" });

  if (req.Items.Any(i => i.Quantity <= 0))
    return Results.BadRequest(new { message = "quantity must be > 0" });

  var client = httpFactory.CreateClient("LegacyMenu");

  // validate restaurant exists in legacy system
  var legacyResponse = await client.GetAsync($"/api/legacy/menu/{req.RestaurantId}", ct);

  if (legacyResponse.StatusCode == HttpStatusCode.NotFound)
    return Results.BadRequest(new { message = "Unknown restaurant (legacy menu)" });

  if (!legacyResponse.IsSuccessStatusCode)
    return Results.StatusCode(502);

  var orderId = Guid.NewGuid();

  // SAFE LOGGING: do not log full req body; avoid PII; keep minimal identifiers only.
  log.LogInformation("Order created {OrderId} for Restaurant {RestaurantId}", orderId, req.RestaurantId);

  return Results.Accepted($"/api/orders/{orderId}", new { orderId, status = "Created" });
})
.WithName("CreateOrder")
.WithOpenApi();

app.Run();

public sealed record CreateOrderRequest(Guid RestaurantId, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid MenuItemId, int Quantity);
