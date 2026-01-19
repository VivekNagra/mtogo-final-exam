namespace Mtogo.LegacyMenu.Api.Models;

public sealed class MenuItem
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
