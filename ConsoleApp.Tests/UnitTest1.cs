using ConsoleApp.Services;

namespace ConsoleApp.Tests;

public class CalculateDiscountTests
{
    private readonly UtilityService _sut = new();

    // A stable weekday reference used across tests (Wednesday)
    private static readonly DateTime Weekday = new(2024, 1, 3); // Wednesday

    // -------------------------------------------------------------------------
    // Guard: orderAmount <= 0
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderAmountIsZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.CalculateDiscount(0m, isPremiumCustomer: false, Weekday));
    }

    [Fact]
    public void OrderAmountIsNegative_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.CalculateDiscount(-100m, isPremiumCustomer: false, Weekday));
    }

    // -------------------------------------------------------------------------
    // Baseline: no discount conditions satisfied
    // -------------------------------------------------------------------------

    [Fact]
    public void NonPremium_AmountAtOrBelow1000_Weekday_ReturnsZeroDiscount()
    {
        decimal result = _sut.CalculateDiscount(500m, isPremiumCustomer: false, Weekday);

        Assert.Equal(0m, result);
    }

    // -------------------------------------------------------------------------
    // Single-condition paths
    // -------------------------------------------------------------------------

    [Fact]
    public void PremiumCustomer_LowVolume_Weekday_AppliesLoyaltyDiscountOnly()
    {
        // isPremiumCustomer=true → +5%; volume ≤ 1000, weekday → no other discount
        decimal result = _sut.CalculateDiscount(500m, isPremiumCustomer: true, Weekday);

        Assert.Equal(500m * 0.05m, result); // 25.00
    }

    [Fact]
    public void NonPremiumCustomer_VolumeAbove1000_Weekday_AppliesVolumeDiscountOnly()
    {
        // orderAmount > 1000 → +4%; not premium, weekday → no other discount
        decimal result = _sut.CalculateDiscount(2000m, isPremiumCustomer: false, Weekday);

        Assert.Equal(2000m * 0.04m, result); // 80.00
    }

    [Fact]
    public void NonPremiumCustomer_LowVolume_Saturday_AppliesWeekendDiscountOnly()
    {
        // MCDC: Saturday=true, Sunday=false → weekend condition true via Saturday
        var saturday = new DateTime(2024, 1, 6); // Saturday
        decimal result = _sut.CalculateDiscount(500m, isPremiumCustomer: false, saturday);

        Assert.Equal(500m * 0.02m, result); // 10.00
    }

    [Fact]
    public void NonPremiumCustomer_LowVolume_Sunday_AppliesWeekendDiscountOnly()
    {
        // MCDC: Saturday=false, Sunday=true → weekend condition true via Sunday
        var sunday = new DateTime(2024, 1, 7); // Sunday
        decimal result = _sut.CalculateDiscount(500m, isPremiumCustomer: false, sunday);

        Assert.Equal(500m * 0.02m, result); // 10.00
    }

    // -------------------------------------------------------------------------
    // Boundary values: volume threshold at 1000
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderAmountExactly1000_NoVolumeDiscount()
    {
        // orderAmount == 1000 → condition (> 1000) is false; no volume discount
        decimal result = _sut.CalculateDiscount(1000m, isPremiumCustomer: false, Weekday);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void OrderAmountJustAbove1000_AppliesVolumeDiscount()
    {
        // orderAmount == 1000.01 → condition (> 1000) is true; volume discount applied
        decimal result = _sut.CalculateDiscount(1000.01m, isPremiumCustomer: false, Weekday);

        Assert.Equal(1000.01m * 0.04m, result); // 40.0004
    }

    // -------------------------------------------------------------------------
    // Combined-discount paths (below cap)
    // -------------------------------------------------------------------------

    [Fact]
    public void PremiumCustomer_VolumeAbove1000_Weekday_CombinesLoyaltyAndVolumeDiscount()
    {
        // 5% + 4% = 9% (below 10% cap)
        decimal result = _sut.CalculateDiscount(2000m, isPremiumCustomer: true, Weekday);

        Assert.Equal(2000m * 0.09m, result); // 180.00
    }

    [Fact]
    public void PremiumCustomer_LowVolume_Weekend_CombinesLoyaltyAndWeekendDiscount()
    {
        // 5% + 2% = 7% (below 10% cap)
        var saturday = new DateTime(2024, 1, 6);
        decimal result = _sut.CalculateDiscount(500m, isPremiumCustomer: true, saturday);

        Assert.Equal(500m * 0.07m, result); // 35.00
    }

    [Fact]
    public void NonPremiumCustomer_VolumeAbove1000_Weekend_CombinesVolumeAndWeekendDiscount()
    {
        // 4% + 2% = 6% (below 10% cap)
        var sunday = new DateTime(2024, 1, 7);
        decimal result = _sut.CalculateDiscount(2000m, isPremiumCustomer: false, sunday);

        Assert.Equal(2000m * 0.06m, result); // 120.00
    }

    // -------------------------------------------------------------------------
    // Cap: all three discounts → 11% → capped at 10%
    // -------------------------------------------------------------------------

    [Fact]
    public void PremiumCustomer_VolumeAbove1000_Weekend_DiscountCappedAtTenPercent()
    {
        // 5% + 4% + 2% = 11% > 10% → cap fires, discount = 10%
        var saturday = new DateTime(2024, 1, 6);
        decimal result = _sut.CalculateDiscount(2000m, isPremiumCustomer: true, saturday);

        Assert.Equal(2000m * 0.10m, result); // 200.00
    }
}
