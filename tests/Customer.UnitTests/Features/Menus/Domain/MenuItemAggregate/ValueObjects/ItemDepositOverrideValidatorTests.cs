namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

public class ItemDepositOverrideValidatorTests
{
    private readonly ItemDepositOverrideValidator _validator = new();

    [Fact]
    public void Validate_WithValidDeposit_ReturnsSuccess()
    {
        var deposit = new TestableItemDepositOverride(50.00m, 5);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeTrue();
    }

    #region DepositAmount Validation

    [Fact]
    public void DepositAmount_WhenZero_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(0m);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.DepositAmountGreaterThanZero);
    }

    [Fact]
    public void DepositAmount_WhenNegative_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(-10.00m);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.DepositAmountGreaterThanZero);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(50.00)]
    [InlineData(1000.00)]
    [InlineData(10000.00)]
    public void DepositAmount_WhenWithinRange_ReturnsSuccess(decimal amount)
    {
        var deposit = new TestableItemDepositOverride(amount);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DepositAmount_WhenExceedsMax_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(10001.00m);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.DepositAmountMax);
    }

    #endregion

    #region MinimumQuantityForDeposit Validation

    [Fact]
    public void MinimumQuantityForDeposit_WhenNull_ReturnsSuccess()
    {
        var deposit = new TestableItemDepositOverride(50.00m, null);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MinimumQuantityForDeposit_WhenZero_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(50.00m, 0);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.MinimumQuantityMin);
    }

    [Fact]
    public void MinimumQuantityForDeposit_WhenNegative_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(50.00m, -1);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.MinimumQuantityMin);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void MinimumQuantityForDeposit_WhenWithinRange_ReturnsSuccess(int quantity)
    {
        var deposit = new TestableItemDepositOverride(50.00m, quantity);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MinimumQuantityForDeposit_WhenExceedsMax_ReturnsError()
    {
        var deposit = new TestableItemDepositOverride(50.00m, 101);

        var result = _validator.Validate(deposit);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ItemDepositOverrideValidationMessages.MinimumQuantityMax);
    }

    #endregion
}
