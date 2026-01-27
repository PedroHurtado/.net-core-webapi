namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class CurrencyValidatorTests
{
    private readonly CurrencyValidator _validator = new();

    [Fact]
    public void Validate_WithValidCurrency_ReturnsSuccess()
    {
        var currency = new TestableCurrency("EUR", "€", 2);

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeTrue();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_ReturnsError()
    {
        var currency = new TestableCurrency("", "€");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeRequired);
    }

    [Theory]
    [InlineData("E")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Code_WhenNotThreeCharacters_ReturnsError(string code)
    {
        var currency = new TestableCurrency(code, "€");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeLength);
    }

    [Theory]
    [InlineData("eur")]
    [InlineData("Eur")]
    [InlineData("eUR")]
    public void Code_WhenNotUppercase_ReturnsError(string code)
    {
        var currency = new TestableCurrency(code, "€");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeUppercase);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Code_WhenValidThreeLetterUppercase_ReturnsSuccess(string code)
    {
        var currency = new TestableCurrency(code, "€");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Symbol Validation

    [Fact]
    public void Symbol_WhenEmpty_ReturnsError()
    {
        var currency = new TestableCurrency("EUR", "");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.SymbolRequired);
    }

    [Fact]
    public void Symbol_WhenExceedsMaxLength_ReturnsError()
    {
        var currency = new TestableCurrency("EUR", "€€€€€€");

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.SymbolMaxLength);
    }

    [Theory]
    [InlineData("€")]
    [InlineData("$")]
    [InlineData("£")]
    [InlineData("¥")]
    public void Symbol_WhenValid_ReturnsSuccess(string symbol)
    {
        var currency = new TestableCurrency("EUR", symbol);

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region DecimalPlaces Validation

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void DecimalPlaces_WhenOutOfRange_ReturnsError(int decimalPlaces)
    {
        var currency = new TestableCurrency("EUR", "€", decimalPlaces);

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.DecimalPlacesRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void DecimalPlaces_WhenInRange_ReturnsSuccess(int decimalPlaces)
    {
        var currency = new TestableCurrency("EUR", "€", decimalPlaces);

        var result = _validator.Validate(currency);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
