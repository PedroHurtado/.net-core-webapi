namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

public class ItemDepositOverrideTests
{
    [Fact]
    public void DepositAmount_SetsCorrectValue()
    {
        var deposit = new TestableItemDepositOverride(50.00m);

        deposit.DepositAmount.Should().Be(50.00m);
    }

    [Fact]
    public void MinimumQuantityForDeposit_SetsCorrectValue()
    {
        var deposit = new TestableItemDepositOverride(50.00m, 5);

        deposit.MinimumQuantityForDeposit.Should().Be(5);
    }

    [Fact]
    public void MinimumQuantityForDeposit_DefaultsToNull()
    {
        var deposit = new TestableItemDepositOverride(50.00m);

        deposit.MinimumQuantityForDeposit.Should().BeNull();
    }

    #region AppliesToAllQuantities

    [Fact]
    public void AppliesToAllQuantities_WhenNoMinimumQuantity_ReturnsTrue()
    {
        var deposit = new TestableItemDepositOverride(50.00m);

        deposit.AppliesToAllQuantities.Should().BeTrue();
    }

    [Fact]
    public void AppliesToAllQuantities_WhenMinimumQuantitySet_ReturnsFalse()
    {
        var deposit = new TestableItemDepositOverride(50.00m, 5);

        deposit.AppliesToAllQuantities.Should().BeFalse();
    }

    #endregion

    #region IsApplicable

    [Fact]
    public void IsApplicable_WhenNoMinimumQuantity_ReturnsTrue()
    {
        var deposit = new TestableItemDepositOverride(50.00m);

        deposit.IsApplicable(1).Should().BeTrue();
        deposit.IsApplicable(100).Should().BeTrue();
    }

    [Theory]
    [InlineData(5, 5, true)]
    [InlineData(5, 6, true)]
    [InlineData(5, 10, true)]
    public void IsApplicable_WhenQuantityMeetsOrExceedsMinimum_ReturnsTrue(int minimum, int quantity, bool expected)
    {
        var deposit = new TestableItemDepositOverride(50.00m, minimum);

        deposit.IsApplicable(quantity).Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(5, 1)]
    [InlineData(10, 9)]
    public void IsApplicable_WhenQuantityBelowMinimum_ReturnsFalse(int minimum, int quantity)
    {
        var deposit = new TestableItemDepositOverride(50.00m, minimum);

        deposit.IsApplicable(quantity).Should().BeFalse();
    }

    #endregion
}
