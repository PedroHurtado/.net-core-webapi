namespace Menus.UnitTests.Features.Menus.Domain.Shared.ValueObjects;

public class PriceOptionValidatorTests
{
    private readonly PriceOptionValidator _validator = new();

    [Fact]
    public void Validate_WithValidPriceOption_ReturnsSuccess()
    {
        var priceOption = new TestablePriceOption(PortionType.Full, 10.00m);

        var result = _validator.Validate(priceOption);

        result.IsValid.Should().BeTrue();
    }

    #region Price Validation

    [Theory]
    [InlineData(PortionType.Small)]
    [InlineData(PortionType.Half)]
    [InlineData(PortionType.Full)]
    public void Price_WhenNullForFixedPortionType_ReturnsError(PortionType portionType)
    {
        var priceOption = new TestablePriceOption(portionType, null);

        var result = _validator.Validate(priceOption);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PriceOptionValidationMessages.PriceRequiredForFixedPortionTypes);
    }

    [Fact]
    public void Price_WhenNullForMarketPrice_ReturnsSuccess()
    {
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, null);

        var result = _validator.Validate(priceOption);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Price_WhenNegative_ReturnsError()
    {
        var priceOption = new TestablePriceOption(PortionType.Full, -1.00m);

        var result = _validator.Validate(priceOption);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PriceOptionValidationMessages.PriceCannotBeNegative);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.00)]
    public void Price_WhenZeroOrPositive_ReturnsSuccess(decimal price)
    {
        var priceOption = new TestablePriceOption(PortionType.Full, price);

        var result = _validator.Validate(priceOption);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
