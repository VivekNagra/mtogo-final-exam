using Mtogo.Ordering.Api.Application;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient<LegacyMenuClient>(client =>
        {
            var baseUrl = builder.Configuration["LegacyMenu:BaseUrl"] ?? "http://localhost:8081";
            client.BaseAddress = new Uri(baseUrl);
        });

        builder.Services.AddScoped<ILegacyMenuClient>(sp => sp.GetRequiredService<LegacyMenuClient>());
        builder.Services.AddScoped<OrderService>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ordering" }));


        app.MapPost("/api/orders", async (CreateOrderRequest req, OrderService svc, CancellationToken ct) =>
        {
            var (ok, status, body) = await svc.CreateOrderAsync(req, ct);
            return Results.Json(body, statusCode: status);
        })
        .WithName("CreateOrder");

        app.Run();
    }
}
