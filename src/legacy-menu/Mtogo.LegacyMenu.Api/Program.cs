using Microsoft.EntityFrameworkCore;
using Mtogo.LegacyMenu.Api.Data;
using Mtogo.LegacyMenu.Api.Repositories;
using Mtogo.LegacyMenu.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Choose DB provider by environment (prevents dual-provider registration)
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<LegacyMenuDbContext>(opt =>
        opt.UseInMemoryDatabase("legacy-menu-testing"));
}
else
{
    builder.Services.AddDbContext<LegacyMenuDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Db")));
}

builder.Services.AddScoped<MenuRepository>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<LegacyMenuDbInitializer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "legacy-menu" }));
app.MapControllers();

// Do not run DB init during tests (WebApplicationFactory)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var init = scope.ServiceProvider.GetRequiredService<LegacyMenuDbInitializer>();
    await init.InitializeAsync(CancellationToken.None);
}

app.Run();

public partial class Program { }
