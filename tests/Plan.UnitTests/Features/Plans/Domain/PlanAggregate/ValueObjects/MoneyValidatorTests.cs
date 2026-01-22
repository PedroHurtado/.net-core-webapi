namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class MoneyValidatorTests
{
    private readonly MoneyValidator _validator = new();

    [Fact]
    public void Validate_WithValidMoney_ReturnsSuccess()
    {
        var money = new TestableMoney(10.00m, Currency.EUR);

        var result = _validator.Validate(money);

        result.IsValid.Should().BeTrue();
    }

    #region Amount Validation

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.00)]
    public void Amount_WhenZeroOrPositive_ReturnsSuccess(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        var result = _validator.Validate(money);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10.00)]
    [InlineData(-100.50)]
    public void Amount_WhenNegative_ReturnsError(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        var result = _validator.Validate(money);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MoneyValidationMessages.AmountCannotBeNegative);
    }

    #endregion

    #region Currency Validation

    [Fact]
    public void Currency_WhenNull_ReturnsError()
    {
        var money = new TestableMoney(10.00m, null!);

        var result = _validator.Validate(money);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MoneyValidationMessages.CurrencyRequired);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Currency_WhenValid_ReturnsSuccess(string currencyCode)
    {
        var currency = Currency.FromCode(currencyCode);
        var money = new TestableMoney(10.00m, currency);

        var result = _validator.Validate(money);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
