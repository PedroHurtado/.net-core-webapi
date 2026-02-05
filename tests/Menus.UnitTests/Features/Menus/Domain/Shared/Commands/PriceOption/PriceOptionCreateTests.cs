namespace Menus.UnitTests.Features.Menus.Domain.Shared.Commands.PriceOption;

public class PriceOptionCreateTests
{
    private readonly PriceOptionValidator _validator = new();
    private readonly PriceOptionVO.Create _create;

    public PriceOptionCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsPriceOption()
    {
        var command = new CreatePriceOptionCommand(PortionType.Full, 10.00m);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.PortionType.Should().Be(PortionType.Full);
        result.Price.Should().Be(10.00m);
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(PortionType.Small)]
    [InlineData(PortionType.Half)]
    [InlineData(PortionType.Full)]
    [InlineData(PortionType.MarketPrice)]
    public void Execute_WithDifferentPortionTypes_SetsPortionType(PortionType portionType)
    {
        decimal? price = portionType == PortionType.MarketPrice ? null : 10.00m;
        var command = new CreatePriceOptionCommand(portionType, price);

        var result = _create.Execute(command);

        result.PortionType.Should().Be(portionType);
    }

    [Fact]
    public void Execute_WithMarketPriceAndNullPrice_ReturnsPriceOption()
    {
        var command = new CreatePriceOptionCommand(PortionType.MarketPrice, null);

        var result = _create.Execute(command);

        result.Price.Should().BeNull();
        result.RequiresMarketPrice.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Execute_WithIsActive_SetsIsActive(bool isActive)
    {
        var command = new CreatePriceOptionCommand(PortionType.Full, 10.00m, isActive);

        var result = _create.Execute(command);

        result.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Execute_WithDefaultIsActive_DefaultsToTrue()
    {
        var command = new CreatePriceOptionCommand(PortionType.Full, 10.00m);

        var result = _create.Execute(command);

        result.IsActive.Should().BeTrue();
    }

    #region Validation Throws

    [Theory]
    [InlineData(PortionType.Small)]
    [InlineData(PortionType.Half)]
    [InlineData(PortionType.Full)]
    public void Execute_WithNullPriceForFixedPortionType_ThrowsValidationException(PortionType portionType)
    {
        var command = new CreatePriceOptionCommand(portionType, null);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativePrice_ThrowsValidationException()
    {
        var command = new CreatePriceOptionCommand(PortionType.Full, -1.00m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
