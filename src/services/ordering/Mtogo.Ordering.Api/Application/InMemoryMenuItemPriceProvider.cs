namespace Mtogo.Ordering.Api.Application;

public sealed class InMemoryMenuItemPriceProvider : IMenuItemPriceProvider
{
    // TODO: Replace with legacy menu price fetch when integration is ready.
    private static readonly IReadOnlyDictionary<Guid, decimal> Prices = new Dictionary<Guid, decimal>
    {
        { Guid.Parse("22222222-2222-2222-2222-222222222222"), 79.00m }, // Classic Burger
        { Guid.Parse("33333333-3333-3333-3333-333333333333"), 29.00m }, // Fries
        { Guid.Parse("44444444-4444-4444-4444-444444444444"), 19.00m }  // Soda
    };

    public bool TryGetPrice(Guid menuItemId, out decimal price)
        => Prices.TryGetValue(menuItemId, out price);
}
