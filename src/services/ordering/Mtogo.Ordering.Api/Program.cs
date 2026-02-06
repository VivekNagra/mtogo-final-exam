using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Domain;
using Mtogo.Ordering.Api.Integration;
using Mtogo.Ordering.Api.Consumers;
using Prometheus;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Tests run in-process (WebApplicationFactory) without external infrastructure.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<PaymentFailedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
           
            var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "rabbitmq";
            cfg.Host(rabbitHost, "/");

                        cfg.ConfigureEndpoints(context);
        });
    });
}

builder.Services.AddHttpClient<LegacyMenuClient>(client =>
{
    var baseUrl = builder.Configuration["LegacyMenu:BaseUrl"] ?? "http://legacy-menu:8080";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<ILegacyMenuClient>(sp => sp.GetRequiredService<LegacyMenuClient>());
builder.Services.AddSingleton<IMenuItemPriceProvider, InMemoryMenuItemPriceProvider>();
builder.Services.AddSingleton<IOrderPricingRules, OrderPricingRules>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.UseHttpMetrics();
app.MapMetrics("/metrics");

// Exposes OpenAPI JSON 
app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ordering" }));

app.MapPost("/api/orders", async (CreateOrderRequest req, OrderService svc, CancellationToken ct) =>
{
    var (ok, status, body) = await svc.CreateOrderAsync(req, ct);
    return Results.Json(body, statusCode: status);
})
.WithName("CreateOrder");

app.Run();

public partial class Program { }