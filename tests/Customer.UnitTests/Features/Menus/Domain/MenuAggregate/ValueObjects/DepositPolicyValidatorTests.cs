namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.ValueObjects;

public class DepositPolicyValidatorTests
{
    private readonly DepositPolicyValidator _validator = new();

    [Fact]
    public void Validate_WithValidFixedAmount_ReturnsSuccess()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidPerPerson_ReturnsSuccess()
    {
        var policy = new TestableDepositPolicy(DepositType.PerPerson, 10m);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidPercentageOfBill_ReturnsSuccess()
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, 25m);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #region Amount Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Amount_WhenZeroOrNegative_ReturnsError(decimal amount)
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, amount);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.AmountGreaterThanZero);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    public void Amount_WhenPositive_ReturnsSuccess(decimal amount)
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, amount);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.AmountGreaterThanZero);
    }

    #endregion

    #region Percentage Validation

    [Fact]
    public void Percentage_WhenPercentageOfBillAndNull_ReturnsError()
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, percentage: null);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.PercentageRequiredForPercentageOfBill);
    }

    [Theory]
    [InlineData(DepositType.FixedAmount)]
    [InlineData(DepositType.PerPerson)]
    public void Percentage_WhenNotPercentageOfBillAndHasValue_ReturnsError(DepositType depositType)
    {
        var policy = new TestableDepositPolicy(depositType, 50m, percentage: 25m);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.PercentageOnlyForPercentageOfBill);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Percentage_WhenOutOfRange_ReturnsError(decimal percentage)
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, percentage);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.PercentageRange);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Percentage_WhenInRange_ReturnsSuccess(decimal percentage)
    {
        var policy = new TestableDepositPolicy(DepositType.PercentageOfBill, 1m, percentage);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.PercentageRange);
    }

    #endregion

    #region MinimumGuestsForDeposit Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MinimumGuestsForDeposit_WhenLessThanOne_ReturnsError(int minimumGuests)
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: minimumGuests);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumGuestsForDepositMin);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void MinimumGuestsForDeposit_WhenOneOrMore_ReturnsSuccess(int minimumGuests)
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: minimumGuests);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumGuestsForDepositMin);
    }

    [Fact]
    public void MinimumGuestsForDeposit_WhenNull_ReturnsSuccess()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumGuestsForDeposit: null);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumGuestsForDepositMin);
    }

    #endregion

    #region MinimumBillForDeposit Validation

    [Fact]
    public void MinimumBillForDeposit_WhenNegative_ReturnsError()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: -1m);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumBillForDepositCannotBeNegative);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(1000)]
    public void MinimumBillForDeposit_WhenZeroOrPositive_ReturnsSuccess(decimal minimumBill)
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: minimumBill);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumBillForDepositCannotBeNegative);
    }

    [Fact]
    public void MinimumBillForDeposit_WhenNull_ReturnsSuccess()
    {
        var policy = new TestableDepositPolicy(DepositType.FixedAmount, 50m, minimumBillForDeposit: null);

        var result = _validator.Validate(policy);

        result.Errors.Should().NotContain(e => e.ErrorMessage == DepositPolicyValidationMessages.MinimumBillForDepositCannotBeNegative);
    }

    #endregion
}
