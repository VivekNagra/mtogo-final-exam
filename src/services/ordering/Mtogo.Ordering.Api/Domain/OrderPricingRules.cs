namespace Mtogo.Ordering.Api.Domain;

public interface IOrderPricingRules
{
    OrderPricingResult CalculateTotal(IEnumerable<PricedOrderItem> items);
}

public sealed class OrderPricingRules : IOrderPricingRules
{
    public OrderPricingResult CalculateTotal(IEnumerable<PricedOrderItem> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));

        var rawSubtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var totalQuantity = items.Sum(i => i.Quantity);

        var rawDeliveryFee = rawSubtotal < 200.00m ? 29.00m : 0.00m;
        var rawDiscount = totalQuantity >= 5 ? rawSubtotal * 0.10m : 0.00m;
        var rawTotal = rawSubtotal + rawDeliveryFee - rawDiscount;

        var subtotal = RoundMoney(rawSubtotal);
        var deliveryFee = RoundMoney(rawDeliveryFee);
        var discount = RoundMoney(rawDiscount);
        var total = RoundMoney(rawTotal);

        return new OrderPricingResult(subtotal, deliveryFee, discount, total);
    }

    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public sealed record PricedOrderItem(Guid MenuItemId, int Quantity, decimal UnitPrice);

public sealed record OrderPricingResult(decimal Subtotal, decimal DeliveryFee, decimal Discount, decimal Total);
