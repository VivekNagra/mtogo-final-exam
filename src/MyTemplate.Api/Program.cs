var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "Large Systems Collaboration Template API",
    status = "running"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    utc = DateTime.UtcNow
}));

app.Run();