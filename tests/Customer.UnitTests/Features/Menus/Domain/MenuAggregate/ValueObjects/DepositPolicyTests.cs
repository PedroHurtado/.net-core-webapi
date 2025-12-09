using Customer.Features.Menus.Domain.MenuAggregate.Enums;
using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;
using FluentAssertions;
using FluentValidation;

namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.ValueObjects;

public class DepositPolicyTests
{
    #region Create Tests

    [Theory]
    [InlineData(DepositType.FixedAmount, 100, null)]
    [InlineData(DepositType.PerPerson, 25, null)]
    [InlineData(DepositType.PercentageOfBill, 50, 20)]
    public void Create_WithValidData_ShouldCreatePolicy(DepositType depositType, decimal amount, int? percentage)
    {
        // Act
        var policy = DepositPolicy.Create(depositType, amount, percentage);

        // Assert
        policy.Should().NotBeNull();
        policy.DepositType.Should().Be(depositType);
        policy.Amount.Should().Be(amount);
        policy.Percentage.Should().Be(percentage);
    }

    [Fact]
    public void Create_WithInvalidData_ShouldThrowValidationException()
    {
        // Act
        var act = () => DepositPolicy.Create(DepositType.FixedAmount, amount: 0);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    #endregion

    #region IsApplicable Tests

    [Fact]
    public void IsApplicable_WhenNoThresholds_ShouldReturnTrue()
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 100);

        // Act
        var result = policy.IsApplicable(guestCount: 1, estimatedBill: 10);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(5, 4)]
    [InlineData(10, 9)]
    public void IsApplicable_WhenGuestCountBelowMinimum_ShouldReturnFalse(int minimumGuests, int guestCount)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 100, minimumGuestsForDeposit: minimumGuests);

        // Act
        var result = policy.IsApplicable(guestCount, estimatedBill: 1000);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(5, 10)]
    [InlineData(1, 1)]
    public void IsApplicable_WhenGuestCountAtOrAboveMinimum_ShouldReturnTrue(int minimumGuests, int guestCount)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 100, minimumGuestsForDeposit: minimumGuests);

        // Act
        var result = policy.IsApplicable(guestCount, estimatedBill: 1000);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, 50)]
    [InlineData(100, 99.99)]
    [InlineData(200, 199)]
    public void IsApplicable_WhenBillBelowMinimum_ShouldReturnFalse(decimal minimumBill, decimal estimatedBill)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 100, minimumBillForDeposit: minimumBill);

        // Act
        var result = policy.IsApplicable(guestCount: 10, estimatedBill);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(100, 200)]
    [InlineData(0, 0)]
    public void IsApplicable_WhenBillAtOrAboveMinimum_ShouldReturnTrue(decimal minimumBill, decimal estimatedBill)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 100, minimumBillForDeposit: minimumBill);

        // Act
        var result = policy.IsApplicable(guestCount: 10, estimatedBill);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(5, 100, 5, 100, true)]   // Ambos cumplidos
    [InlineData(5, 100, 5, 50, false)]   // Solo guests cumplido
    [InlineData(5, 100, 3, 100, false)]  // Solo bill cumplido
    [InlineData(5, 100, 3, 50, false)]   // Ninguno cumplido
    public void IsApplicable_WhenBothThresholdsConfigured_ShouldRequireBoth(
        int minimumGuests, decimal minimumBill, int guestCount, decimal estimatedBill, bool expected)
    {
        // Arrange
        var policy = DepositPolicy.Create(
            DepositType.FixedAmount,
            100,
            minimumGuestsForDeposit: minimumGuests,
            minimumBillForDeposit: minimumBill);

        // Act
        var result = policy.IsApplicable(guestCount, estimatedBill);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region CalculateDeposit Tests

    [Theory]
    [InlineData(50, 1)]
    [InlineData(50, 10)]
    [InlineData(100, 5)]
    public void CalculateDeposit_WhenFixedAmount_ShouldReturnAmount(decimal amount, int guestCount)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.FixedAmount, amount);

        // Act
        var result = policy.CalculateDeposit(guestCount, estimatedBill: 1000);

        // Assert
        result.Should().Be(amount);
    }

    [Theory]
    [InlineData(10, 1, 10)]
    [InlineData(10, 5, 50)]
    [InlineData(15, 4, 60)]
    [InlineData(25, 10, 250)]
    public void CalculateDeposit_WhenPerPerson_ShouldReturnAmountTimesGuests(
        decimal amount, int guestCount, decimal expected)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.PerPerson, amount);

        // Act
        var result = policy.CalculateDeposit(guestCount, estimatedBill: 1000);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 100, 10)]
    [InlineData(50, 200, 100)]
    [InlineData(100, 50, 50)]
    [InlineData(25, 400, 100)]
    public void CalculateDeposit_WhenPercentageOfBill_ShouldReturnPercentage(
        decimal percentage, decimal estimatedBill, decimal expected)
    {
        // Arrange
        var policy = DepositPolicy.Create(DepositType.PercentageOfBill, amount: 1, percentage: percentage);

        // Act
        var result = policy.CalculateDeposit(guestCount: 1, estimatedBill);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateDeposit_WhenUnknownDepositType_ShouldReturnZero()
    {
        // Arrange
        var unknownDepositType = (DepositType)999;
        var policy = CreatePolicyWithReflection(unknownDepositType, amount: 100);

        // Act
        var result = policy.CalculateDeposit(guestCount: 5, estimatedBill: 200);

        // Assert
        result.Should().Be(0m);
    }

    #endregion

    #region Helper Methods

    private static DepositPolicy CreatePolicyWithReflection(
        DepositType depositType,
        decimal amount,
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
