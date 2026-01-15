namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.ValueObjects;

public class DepositPolicyTests
{
    #region Constructor

    [Fact]
    public void Constructor_SetsDepositType()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        policy.DepositType.Should().Be(DepositType.FixedAmount);
    }

    [Fact]
    public void Constructor_SetsAmount()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        policy.Amount.Should().Be(50m);
    }

    [Fact]
    public void Constructor_SetsPercentage()
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, 25m);

        policy.Percentage.Should().Be(25m);
    }

    [Fact]
    public void Constructor_SetsMinimumBillForDeposit()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 100m);

        policy.MinimumBillForDeposit.Should().Be(100m);
    }

    [Fact]
    public void Constructor_SetsMinimumGuestsForDeposit()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: 5);

        policy.MinimumGuestsForDeposit.Should().Be(5);
    }

    #endregion

    #region IsApplicable

    [Fact]
    public void IsApplicable_WithNoThresholds_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        policy.IsApplicable(guestCount: 2, estimatedBill: 100m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenGuestCountBelowMinimum_ReturnsFalse()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: 5);

        policy.IsApplicable(guestCount: 3, estimatedBill: 100m).Should().BeFalse();
    }

    [Fact]
    public void IsApplicable_WhenGuestCountAtMinimum_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: 5);

        policy.IsApplicable(guestCount: 5, estimatedBill: 100m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenGuestCountAboveMinimum_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: 5);

        policy.IsApplicable(guestCount: 10, estimatedBill: 100m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenBillBelowMinimum_ReturnsFalse()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 200m);

        policy.IsApplicable(guestCount: 2, estimatedBill: 100m).Should().BeFalse();
    }

    [Fact]
    public void IsApplicable_WhenBillAtMinimum_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 200m);

        policy.IsApplicable(guestCount: 2, estimatedBill: 200m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenBillAboveMinimum_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 200m);

        policy.IsApplicable(guestCount: 2, estimatedBill: 300m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenBothThresholdsMetAtMinimum_ReturnsTrue()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 200m, minimumGuestsForDeposit: 5);

        policy.IsApplicable(guestCount: 5, estimatedBill: 200m).Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenGuestThresholdNotMet_ReturnsFalse()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: 200m, minimumGuestsForDeposit: 5);

        policy.IsApplicable(guestCount: 3, estimatedBill: 300m).Should().BeFalse();
    }

    #endregion

    #region CalculateDeposit

    [Fact]
    public void CalculateDeposit_FixedAmount_ReturnsAmount()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        policy.CalculateDeposit(guestCount: 10, estimatedBill: 500m).Should().Be(50m);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    [InlineData(10, 100)]
    public void CalculateDeposit_PerPerson_ReturnsAmountTimesGuests(int guestCount, decimal expected)
    {
        var policy = new TestableDepositPolicy(DepositType.PerPerson, 10m);

        policy.CalculateDeposit(guestCount, estimatedBill: 500m).Should().Be(expected);
    }

    [Theory]
    [InlineData(100, 10)]
    [InlineData(200, 20)]
    [InlineData(500, 50)]
    public void CalculateDeposit_PercentageOfBill_ReturnsPercentageOfBill(decimal estimatedBill, decimal expected)
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, 10m);

        policy.CalculateDeposit(guestCount: 2, estimatedBill).Should().Be(expected);
    }

    #endregion
}
