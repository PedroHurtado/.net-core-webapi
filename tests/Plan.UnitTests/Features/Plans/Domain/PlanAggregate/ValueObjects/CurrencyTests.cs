namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class CurrencyTests
{
    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Code_SetsCorrectValue(string code)
    {
        var currency = new TestableCurrency(code, "€");

        currency.Code.Should().Be(code);
    }

    [Theory]
    [InlineData("€")]
    [InlineData("$")]
    [InlineData("£")]
    public void Symbol_SetsCorrectValue(string symbol)
    {
        var currency = new TestableCurrency("EUR", symbol);

        currency.Symbol.Should().Be(symbol);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void DecimalPlaces_SetsCorrectValue(int decimalPlaces)
    {
        var currency = new TestableCurrency("EUR", "€", decimalPlaces);

        currency.DecimalPlaces.Should().Be(decimalPlaces);
    }

    #region Static Instances

    [Fact]
    public void EUR_ReturnsCorrectCurrency()
    {
        var eur = Currency.EUR;

        eur.Code.Should().Be("EUR");
        eur.Symbol.Should().Be("€");
        eur.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void USD_ReturnsCorrectCurrency()
    {
        var usd = Currency.USD;

        usd.Code.Should().Be("USD");
        usd.Symbol.Should().Be("$");
        usd.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void GBP_ReturnsCorrectCurrency()
    {
        var gbp = Currency.GBP;

        gbp.Code.Should().Be("GBP");
        gbp.Symbol.Should().Be("£");
        gbp.DecimalPlaces.Should().Be(2);
    }

    #endregion

    #region FromCode

    [Theory]
    [InlineData("EUR")]
    [InlineData("eur")]
    [InlineData("EuR")]
    public void FromCode_WithEUR_ReturnsEURCurrency(string code)
    {
        var currency = Currency.FromCode(code);

        currency.Should().Be(Currency.EUR);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("usd")]
    [InlineData("UsD")]
    public void FromCode_WithUSD_ReturnsUSDCurrency(string code)
    {
        var currency = Currency.FromCode(code);

        currency.Should().Be(Currency.USD);
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("gbp")]
    [InlineData("GbP")]
    public void FromCode_WithGBP_ReturnsGBPCurrency(string code)
    {
        var currency = Currency.FromCode(code);

        currency.Should().Be(Currency.GBP);
    }

    [Fact]
    public void FromCode_WithUnsupportedCode_ThrowsArgumentException()
    {
        var act = () => Currency.FromCode("JPY");

        act.Should().Throw<ValidationException>()
            .WithMessage("*Currency JPY not supported*");
    }

    #endregion
}
