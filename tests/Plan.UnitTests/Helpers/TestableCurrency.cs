namespace Plans.UnitTests.Helpers;

public record TestableCurrency : Currency
{
    public TestableCurrency(string code, string symbol, int decimalPlaces = 2)
        : base(code, symbol, decimalPlaces) { }
}
