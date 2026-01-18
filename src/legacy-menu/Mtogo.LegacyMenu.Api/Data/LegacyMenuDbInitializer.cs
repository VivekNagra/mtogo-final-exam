using Microsoft.EntityFrameworkCore;
using Mtogo.LegacyMenu.Api.Models;

namespace Mtogo.LegacyMenu.Api.Data;

public sealed class LegacyMenuDbInitializer
{
  private readonly LegacyMenuDbContext _db;
  private readonly ILogger<LegacyMenuDbInitializer> _log;

  public LegacyMenuDbInitializer(LegacyMenuDbContext db, ILogger<LegacyMenuDbInitializer> log)
  {
    _db = db;
    _log = log;
  }

  public async Task InitializeAsync(CancellationToken ct)
  {
    // For exam simplicity: ensure schema exists (no manual migrations needed yet)
    await _db.Database.EnsureCreatedAsync(ct);

    if (await _db.Restaurants.AnyAsync(ct))
    {
      _log.LogInformation("LegacyMenu DB already seeded.");
      return;
    }

    var restaurant = new Restaurant
    {
      Id = SeedIds.RestaurantId,
      Name = "Legacy Burger House"
    };

    var items = new[]
    {
      new MenuItem { Id = SeedIds.BurgerId, RestaurantId = restaurant.Id, Name = "Classic Burger", Price = 79.00m },
      new MenuItem { Id = SeedIds.FriesId,  RestaurantId = restaurant.Id, Name = "Fries",          Price = 29.00m },
      new MenuItem { Id = SeedIds.SodaId,   RestaurantId = restaurant.Id, Name = "Soda",           Price = 19.00m }
    };

    _db.Restaurants.Add(restaurant);
    _db.MenuItems.AddRange(items);

    await _db.SaveChangesAsync(ct);

    _log.LogInformation("Seeded LegacyMenu DB with RestaurantId={RestaurantId}", restaurant.Id);
  }
}
