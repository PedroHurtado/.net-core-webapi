namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Currency;

public class CurrencyCreateTests
{
    private readonly CurrencyValidator _validator = new();
    private readonly CurrencyVO.Create _create;

    public CurrencyCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsCurrency()
    {
        var command = new CreateCurrencyCommand("EUR", "€", 2);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Code.Should().Be("EUR");
        result.Symbol.Should().Be("€");
        result.DecimalPlaces.Should().Be(2);
    }

    [Theory]
    [InlineData("EUR", "€")]
    [InlineData("USD", "$")]
    [InlineData("GBP", "£")]
    public void Execute_WithDifferentCurrencies_SetsCorrectValues(string code, string symbol)
    {
        var command = new CreateCurrencyCommand(code, symbol);

        var result = _create.Execute(command);

        result.Code.Should().Be(code);
        result.Symbol.Should().Be(symbol);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void Execute_WithDecimalPlaces_SetsDecimalPlaces(int decimalPlaces)
    {
        var command = new CreateCurrencyCommand("EUR", "€", decimalPlaces);

        var result = _create.Execute(command);

        result.DecimalPlaces.Should().Be(decimalPlaces);
    }

    [Fact]
    public void Execute_WithDefaultDecimalPlaces_SetsToTwo()
    {
        var command = new CreateCurrencyCommand("EUR", "€");

        var result = _create.Execute(command);

        result.DecimalPlaces.Should().Be(2);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyCode_ThrowsValidationException()
    {
        var command = new CreateCurrencyCommand("", "€");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("E")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Execute_WithInvalidCodeLength_ThrowsValidationException(string code)
    {
        var command = new CreateCurrencyCommand(code, "€");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("eur")]
    [InlineData("Eur")]
    [InlineData("eUR")]
    public void Execute_WithNonUppercaseCode_ThrowsValidationException(string code)
    {
        var command = new CreateCurrencyCommand(code, "€");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptySymbol_ThrowsValidationException()
    {
        var command = new CreateCurrencyCommand("EUR", "");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithSymbolTooLong_ThrowsValidationException()
    {
        var command = new CreateCurrencyCommand("EUR", "€€€€€€");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Execute_WithDecimalPlacesOutOfRange_ThrowsValidationException(int decimalPlaces)
    {
        var command = new CreateCurrencyCommand("EUR", "€", decimalPlaces);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
