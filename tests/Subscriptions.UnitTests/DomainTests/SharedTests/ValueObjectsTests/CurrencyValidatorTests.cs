namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class CurrencyValidatorTests
{
    private readonly CurrencyValidator _validator = new();

    [Fact]
    public void Validate_WithValidCurrency_ReturnsSuccess()
    {
        var vo = new Currency("EUR", "€", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_ReturnsError()
    {
        var vo = new Currency("", "€", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeRequired);
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Code_WhenWrongLength_ReturnsError(string code)
    {
        var vo = new Currency(code, "€", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeLength);
    }

    [Fact]
    public void Code_WhenLowercase_ReturnsError()
    {
        var vo = new Currency("eur", "€", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.CodeUppercase);
    }

    #endregion

    #region Symbol Validation

    [Fact]
    public void Symbol_WhenEmpty_ReturnsError()
    {
        var vo = new Currency("EUR", "", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.SymbolRequired);
    }

    [Fact]
    public void Symbol_WhenExceedsMaxLength_ReturnsError()
    {
        var vo = new Currency("EUR", "€€€€€€", 2);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.SymbolMaxLength);
    }

    #endregion

    #region DecimalPlaces Validation

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void DecimalPlaces_WhenOutOfRange_ReturnsError(int decimalPlaces)
    {
        var vo = new Currency("EUR", "€", decimalPlaces);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CurrencyValidationMessages.DecimalPlacesRange);
    }

    #endregion
}
