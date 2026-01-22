namespace Plan.UnitTests.Helpers;

public record TestableMoney : Money
{
    public TestableMoney(decimal amount, Currency currency)
        : base(amount, currency) { }
}
