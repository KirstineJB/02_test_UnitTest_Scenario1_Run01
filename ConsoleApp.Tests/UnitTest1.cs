namespace ConsoleApp.Tests;

using ConsoleApp.Services;

public class UtilityServiceTests
{
    private readonly UtilityService _sut = new();

    // Fixed dates for deterministic weekend/weekday tests
    private static readonly DateTime Saturday = new(2025, 3, 8);
    private static readonly DateTime Sunday = new(2025, 3, 9);
    private static readonly DateTime Monday = new(2025, 3, 10);

    #region ArgumentException (orderAmount validation)

    [Fact]
    public void CalculateDiscount_OrderAmountZero_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _sut.CalculateDiscount(0, false, Monday));

        Assert.Equal("Order amount must be positive", ex.Message);
    }

    [Fact]
    public void CalculateDiscount_OrderAmountNegative_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _sut.CalculateDiscount(-1, false, Monday));

        Assert.Equal("Order amount must be positive", ex.Message);
    }

    [Fact]
    public void CalculateDiscount_OrderAmountPositive_DoesNotThrow()
    {
        var result = _sut.CalculateDiscount(0.01m, false, Monday);

        Assert.Equal(0, result);
    }

    #endregion

    #region Loyalty discount (isPremiumCustomer)

    [Fact]
    public void CalculateDiscount_PremiumCustomer_AppliesLoyaltyDiscount()
    {
        var result = _sut.CalculateDiscount(100, true, Monday);

        Assert.Equal(5, result, 2);
    }

    [Fact]
    public void CalculateDiscount_NonPremiumCustomer_NoLoyaltyDiscount()
    {
        var result = _sut.CalculateDiscount(100, false, Monday);

        Assert.Equal(0, result);
    }

    #endregion

    #region Volume discount (orderAmount > 1000)

    [Fact]
    public void CalculateDiscount_OrderAmountAbove1000_AppliesVolumeDiscount()
    {
        var result = _sut.CalculateDiscount(1500, false, Monday);

        Assert.Equal(60, result, 2);
    }

    [Fact]
    public void CalculateDiscount_OrderAmountExactly1000_NoVolumeDiscount()
    {
        var result = _sut.CalculateDiscount(1000, false, Monday);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateDiscount_OrderAmountJustAbove1000_AppliesVolumeDiscount()
    {
        var result = _sut.CalculateDiscount(1000.01m, false, Monday);

        Assert.Equal(40.0004m, result, 4);
    }

    #endregion

    #region Weekend campaign (MCDC for compound OR)

    [Fact]
    public void CalculateDiscount_Saturday_AppliesWeekendDiscount()
    {
        var result = _sut.CalculateDiscount(100, false, Saturday);

        Assert.Equal(2, result, 2);
    }

    [Fact]
    public void CalculateDiscount_Sunday_AppliesWeekendDiscount()
    {
        var result = _sut.CalculateDiscount(100, false, Sunday);

        Assert.Equal(2, result, 2);
    }

    [Fact]
    public void CalculateDiscount_Weekday_NoWeekendDiscount()
    {
        var result = _sut.CalculateDiscount(100, false, Monday);

        Assert.Equal(0, result);
    }

    #endregion

    #region Discount cap (discount > 0.10)

    [Fact]
    public void CalculateDiscount_CombinedDiscountsExceedCap_CapsAtTenPercent()
    {
        var result = _sut.CalculateDiscount(1500, true, Saturday);

        Assert.Equal(150, result, 2);
    }

    [Fact]
    public void CalculateDiscount_CombinedDiscountsBelowCap_NoCapApplied()
    {
        var result = _sut.CalculateDiscount(1500, true, Monday);

        Assert.Equal(135, result, 2);
    }

    #endregion

    #region Combined scenarios and edge cases

    [Fact]
    public void CalculateDiscount_NoDiscounts_ReturnsZero()
    {
        var result = _sut.CalculateDiscount(500, false, Monday);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateDiscount_AllDiscountsBelowCap_ReturnsSumOfDiscounts()
    {
        // 500 < 1000 so no volume discount; premium (0.05) + weekend (0.02) = 0.07
        var result = _sut.CalculateDiscount(500, true, Saturday);

        Assert.Equal(35, result, 2);
    }

    [Fact]
    public void CalculateDiscount_MinimumPositiveAmount_CalculatesCorrectly()
    {
        var result = _sut.CalculateDiscount(0.01m, true, Saturday);

        Assert.Equal(0.0007m, result, 4);
    }

    #endregion
}
