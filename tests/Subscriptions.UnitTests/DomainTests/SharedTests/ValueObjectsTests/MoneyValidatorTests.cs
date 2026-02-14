namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class MoneyValidatorTests
{
    private readonly MoneyValidator _validator = new();

    [Fact]
    public void Validate_WithValidMoney_ReturnsSuccess()
    {
        var vo = new Money(9.99m, Currency.EUR);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroAmount_ReturnsSuccess()
    {
        var vo = new Money(0m, Currency.EUR);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #region Amount Validation

    [Fact]
    public void Amount_WhenNegative_ReturnsError()
    {
        var vo = new Money(-5m, Currency.EUR);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MoneyValidationMessages.AmountCannotBeNegative);
    }

    #endregion

    #region Currency Validation

    [Fact]
    public void Currency_WhenNull_ReturnsError()
    {
        var vo = new Money(9.99m, null!);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MoneyValidationMessages.CurrencyRequired);
    }

    #endregion
}
