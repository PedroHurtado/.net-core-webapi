namespace Customer.UnitTests.Features.Menus.Domain.Shared.ValueObjects;

public class PriceOptionTests
{
    [Theory]
    [InlineData(PortionType.Small)]
    [InlineData(PortionType.Half)]
    [InlineData(PortionType.Full)]
    [InlineData(PortionType.MarketPrice)]
    public void PortionType_SetsCorrectValue(PortionType portionType)
    {
        var priceOption = new TestablePriceOption(portionType, 10.00m);

        priceOption.PortionType.Should().Be(portionType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(99.99)]
    public void Price_SetsCorrectValue(decimal price)
    {
        var priceOption = new TestablePriceOption(PortionType.Full, price);

        priceOption.Price.Should().Be(price);
    }

    [Fact]
    public void Price_WhenNull_SetsNull()
    {
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, null);

        priceOption.Price.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsActive_SetsCorrectValue(bool isActive)
    {
        var priceOption = new TestablePriceOption(PortionType.Full, 10.00m, isActive);

        priceOption.IsActive.Should().Be(isActive);
    }

    #region RequiresMarketPrice

    [Fact]
    public void RequiresMarketPrice_WhenMarketPriceAndNullPrice_ReturnsTrue()
    {
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, null);

        priceOption.RequiresMarketPrice.Should().BeTrue();
    }

    [Fact]
    public void RequiresMarketPrice_WhenMarketPriceWithFixedPrice_ReturnsFalse()
    {
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, 15.00m);

        priceOption.RequiresMarketPrice.Should().BeFalse();
    }

    [Theory]
    [InlineData(PortionType.Small)]
    [InlineData(PortionType.Half)]
    [InlineData(PortionType.Full)]
    public void RequiresMarketPrice_WhenFixedPortionType_ReturnsFalse(PortionType portionType)
    {
        var priceOption = new TestablePriceOption(portionType, 10.00m);

        priceOption.RequiresMarketPrice.Should().BeFalse();
    }

    #endregion

    #region DisplayPrice

    [Fact]
    public void DisplayPrice_WhenRequiresMarketPrice_ReturnsSubjectToMarket()
    {
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, null);

        priceOption.DisplayPrice.Should().Be("S/M");
    }

    [Fact]
    public void DisplayPrice_WhenHasPrice_ReturnsFormattedCurrency()
    {
        var priceOption = new TestablePriceOption(PortionType.Full, 10.50m);

        priceOption.DisplayPrice.Should().Contain("10");
    }

    #endregion
}
