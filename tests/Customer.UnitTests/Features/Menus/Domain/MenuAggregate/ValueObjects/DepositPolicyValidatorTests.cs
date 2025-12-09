using Customer.Features.Menus.Domain.MenuAggregate.Enums;
using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.ValueObjects;

public class DepositPolicyValidatorTests
{
    private readonly DepositPolicyValidator _validator = new();

    #region Amount Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Amount_WhenZeroOrNegative_ShouldFail(decimal amount)
    {
        // Arrange
        var policy = CreatePolicy(amount: amount);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(1000)]
    public void Amount_WhenPositive_ShouldPass(decimal amount)
    {
        // Arrange
        var policy = CreatePolicy(amount: amount);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    #endregion

    #region Percentage + DepositType Tests

    [Fact]
    public void Percentage_WhenPercentageOfBill_AndNull_ShouldFail()
    {
        // Arrange
        var policy = CreatePolicy(depositType: DepositType.PercentageOfBill, percentage: null);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Percentage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Percentage_WhenPercentageOfBill_AndValid_ShouldPass(decimal percentage)
    {
        // Arrange
        var policy = CreatePolicy(depositType: DepositType.PercentageOfBill, percentage: percentage);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Percentage);
    }

    [Theory]
    [InlineData(DepositType.PerPerson)]
    [InlineData(DepositType.FixedAmount)]
    public void Percentage_WhenNotPercentageOfBill_AndHasValue_ShouldFail(DepositType depositType)
    {
        // Arrange
        var policy = CreatePolicy(depositType: depositType, percentage: 50);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Percentage);
    }

    [Theory]
    [InlineData(DepositType.PerPerson)]
    [InlineData(DepositType.FixedAmount)]
    public void Percentage_WhenNotPercentageOfBill_AndNull_ShouldPass(DepositType depositType)
    {
        // Arrange
        var policy = CreatePolicy(depositType: depositType, percentage: null);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Percentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.99)]
    [InlineData(101)]
    [InlineData(150)]
    public void Percentage_WhenOutOfRange_ShouldFail(decimal percentage)
    {
        // Arrange
        var policy = CreatePolicy(depositType: DepositType.PercentageOfBill, percentage: percentage);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Percentage);
    }

    #endregion

    #region MinimumGuestsForDeposit Tests

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(5)]
    public void MinimumGuestsForDeposit_WhenNullOrValid_ShouldPass(int? minimumGuests)
    {
        // Arrange
        var policy = CreatePolicy(minimumGuestsForDeposit: minimumGuests);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinimumGuestsForDeposit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MinimumGuestsForDeposit_WhenLessThan1_ShouldFail(int minimumGuests)
    {
        // Arrange
        var policy = CreatePolicy(minimumGuestsForDeposit: minimumGuests);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinimumGuestsForDeposit);
    }

    #endregion

    #region MinimumBillForDeposit Tests

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(100)]
    public void MinimumBillForDeposit_WhenNullOrNonNegative_ShouldPass(int? minimumBill)
    {
        // Arrange
        var policy = CreatePolicy(minimumBillForDeposit: minimumBill);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinimumBillForDeposit);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void MinimumBillForDeposit_WhenNegative_ShouldFail(decimal minimumBill)
    {
        // Arrange
        var policy = CreatePolicy(minimumBillForDeposit: minimumBill);

        // Act
        var result = _validator.TestValidate(policy);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinimumBillForDeposit);
    }

    #endregion

    #region Helper Methods

    private static DepositPolicy CreatePolicy(
        DepositType depositType = DepositType.FixedAmount,
        decimal amount = 10m,
        decimal? percentage = null,
        decimal? minimumBillForDeposit = null,
        int? minimumGuestsForDeposit = null)
    {
        return (DepositPolicy)Activator.CreateInstance(
            typeof(DepositPolicy),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [depositType, amount, percentage, minimumBillForDeposit, minimumGuestsForDeposit],
            null)!;
    }

    #endregion
}
