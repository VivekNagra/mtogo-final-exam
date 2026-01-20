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

    [Fact]
    public void CalculateTotal_RoundsMoneyAwayFromZero()
    {
        var items = new[] { new PricedOrderItem(Guid.NewGuid(), 5, 0.01m) };

        var result = _rules.CalculateTotal(items);

        Assert.Equal(0.05m, result.Subtotal);
        Assert.Equal(29.00m, result.DeliveryFee);
        Assert.Equal(0.01m, result.Discount);
        Assert.Equal(29.05m, result.Total);
    }

    [Fact]
    public void CalculateTotal_Throws_WhenItemsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _rules.CalculateTotal(null!));
    }

    [Fact]
    public void CalculateTotal_NoDiscount_NoDeliveryFee_WhenSubtotalAboveThreshold()
    {
        var items = new[] { new PricedOrderItem(Guid.NewGuid(), 4, 60.00m) };

        var result = _rules.CalculateTotal(items);

        Assert.Equal(240.00m, result.Subtotal);
        Assert.Equal(0.00m, result.DeliveryFee);
        Assert.Equal(0.00m, result.Discount);
        Assert.Equal(240.00m, result.Total);
    }

    [Fact]
    public void CalculateTotal_AppliesDiscountWithoutDeliveryFee_WhenBulkAndOverThreshold()
    {
        var items = new[] { new PricedOrderItem(Guid.NewGuid(), 5, 50.00m) }; // subtotal 250

        var result = _rules.CalculateTotal(items);

        Assert.Equal(250.00m, result.Subtotal);
        Assert.Equal(0.00m, result.DeliveryFee);
        Assert.Equal(25.00m, result.Discount);
        Assert.Equal(225.00m, result.Total);
    }
}
