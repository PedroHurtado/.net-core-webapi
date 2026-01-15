namespace Customer.UnitTests.Helpers;

public record TestablePriceOption : PriceOption
{
    public TestablePriceOption(PortionType portionType, decimal? price, bool isActive = true)
        : base(portionType, price, isActive) { }
}