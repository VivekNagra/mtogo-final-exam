using Microsoft.AspNetCore.Mvc;
using Mtogo.LegacyMenu.Api.Services;

namespace Mtogo.LegacyMenu.Api.Controllers;

[ApiController]
[Route("api/legacy/menu")]
public sealed class MenuController : ControllerBase
{
    private readonly MenuService _menu;

    public MenuController(MenuService menu) => _menu = menu;

    [HttpGet("{restaurantId:guid}")]
    public async Task<IActionResult> GetMenu(Guid restaurantId, CancellationToken ct)
    {
        if (!await _menu.RestaurantExists(restaurantId, ct))
            return NotFound(new { message = "Restaurant not found" });

        var items = await _menu.GetMenu(restaurantId, ct);
        return Ok(items.Select(i => new { i.Id, i.Name, i.Price }));
    }

    [HttpGet("item/{menuItemId:guid}")]
    public async Task<IActionResult> GetMenuItem(Guid menuItemId, CancellationToken ct)
    {
        var item = await _menu.GetMenuItem(menuItemId, ct);
        if (item is null) return NotFound(new { message = "Menu item not found" });

        return Ok(new { item.Id, item.RestaurantId, item.Name, item.Price });
    }
}
