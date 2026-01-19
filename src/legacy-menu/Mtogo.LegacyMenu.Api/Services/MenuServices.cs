using Mtogo.LegacyMenu.Api.Models;
using Mtogo.LegacyMenu.Api.Repositories;

namespace Mtogo.LegacyMenu.Api.Services;

public sealed class MenuService
{
    private readonly MenuRepository _repo;

    public MenuService(MenuRepository repo) => _repo = repo;

    public Task<bool> RestaurantExists(Guid restaurantId, CancellationToken ct) =>
      _repo.RestaurantExists(restaurantId, ct);

    public Task<List<MenuItem>> GetMenu(Guid restaurantId, CancellationToken ct) =>
      _repo.GetMenu(restaurantId, ct);

    public Task<MenuItem?> GetMenuItem(Guid menuItemId, CancellationToken ct) =>
      _repo.GetMenuItem(menuItemId, ct);
}
