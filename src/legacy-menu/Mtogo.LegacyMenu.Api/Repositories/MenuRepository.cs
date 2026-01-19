using Microsoft.EntityFrameworkCore;
using Mtogo.LegacyMenu.Api.Data;
using Mtogo.LegacyMenu.Api.Models;

namespace Mtogo.LegacyMenu.Api.Repositories;

public sealed class MenuRepository
{
    private readonly LegacyMenuDbContext _db;

    public MenuRepository(LegacyMenuDbContext db) => _db = db;

    public Task<bool> RestaurantExists(Guid restaurantId, CancellationToken ct) =>
      _db.Restaurants.AnyAsync(r => r.Id == restaurantId, ct);

    public Task<List<MenuItem>> GetMenu(Guid restaurantId, CancellationToken ct) =>
      _db.MenuItems.Where(m => m.RestaurantId == restaurantId).ToListAsync(ct);

    public Task<MenuItem?> GetMenuItem(Guid menuItemId, CancellationToken ct) =>
      _db.MenuItems.FirstOrDefaultAsync(m => m.Id == menuItemId, ct);
}
