using MassTransit;
using Mtogo.Payment.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/");

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "payment-service" }));

app.Run();