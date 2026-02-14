namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class CurrencyTests
{
    #region Code

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Code_SetsCorrectValue(string code)
    {
        var vo = new Currency(code, "€", 2);

        vo.Code.Should().Be(code);
    }

    #endregion

    #region Symbol

    [Theory]
    [InlineData("€")]
    [InlineData("$")]
    [InlineData("£")]
    public void Symbol_SetsCorrectValue(string symbol)
    {
        var vo = new Currency("EUR", symbol, 2);

        vo.Symbol.Should().Be(symbol);
    }

    #endregion

    #region DecimalPlaces

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void DecimalPlaces_SetsCorrectValue(int decimalPlaces)
    {
        var vo = new Currency("EUR", "€", decimalPlaces);

        vo.DecimalPlaces.Should().Be(decimalPlaces);
    }

    #endregion
}
