using Mtogo.Ordering.Api.Domain;

namespace Mtogo.Ordering.Tests;

public sealed class OrderPricingRulesTests
{
    private readonly OrderPricingRules _rules = new();

    public static IEnumerable<object[]> PricingCases()
    {
        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 1, 199.99m) },
            199.99m, 29.00m, 0.00m, 228.99m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 1, 200.00m) },
            200.00m, 0.00m, 0.00m, 200.00m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 1, 200.01m) },
            200.01m, 0.00m, 0.00m, 200.01m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 4, 25.00m) },
            100.00m, 29.00m, 0.00m, 129.00m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 5, 20.00m) },
            100.00m, 29.00m, 10.00m, 119.00m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 5, 39.80m) },
            199.00m, 29.00m, 19.90m, 208.10m
        };

        yield return new object[]
        {
            new[] { new PricedOrderItem(Guid.NewGuid(), 5, 40.00m) },
            200.00m, 0.00m, 20.00m, 180.00m
        };
    }

    [Theory]
    [MemberData(nameof(PricingCases))]
    public void CalculateTotal_AppliesDeliveryFeeAndBulkDiscount(
        IReadOnlyCollection<PricedOrderItem> items,
        decimal expectedSubtotal,
        decimal expectedFee,
        decimal expectedDiscount,
        decimal expectedTotal)
    {
        var result = _rules.CalculateTotal(items);

        Assert.Equal(expectedSubtotal, result.Subtotal);
        Assert.Equal(expectedFee, result.DeliveryFee);
        Assert.Equal(expectedDiscount, result.Discount);
        Assert.Equal(expectedTotal, result.Total);
    }
}
