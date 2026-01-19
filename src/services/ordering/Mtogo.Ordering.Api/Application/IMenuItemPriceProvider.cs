namespace Mtogo.Ordering.Api.Application;

public interface IMenuItemPriceProvider
{
    bool TryGetPrice(Guid menuItemId, out decimal price);
}
