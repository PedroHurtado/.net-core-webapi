namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class PriceRangeCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly PriceRange.Create _create = fixture.Get<PriceRange.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsPriceRange()
    {
        var command = new CreatePriceRangeCommand(10m, 20m);

        var result = _create.Execute(command);

        result.MinPrice.Should().Be(10m);
        result.MaxPrice.Should().Be(20m);
    }

    [Fact]
    public void Execute_WithSameMinAndMax_ReturnsPriceRange()
    {
        var command = new CreatePriceRangeCommand(15m, 15m);

        var result = _create.Execute(command);

        result.MinPrice.Should().Be(15m);
        result.MaxPrice.Should().Be(15m);
    }

    [Fact]
    public void Execute_WithNegativeMinPrice_ThrowsValidationException()
    {
        var command = new CreatePriceRangeCommand(-5m, 20m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{PriceRangeValidationMessages.MinPriceNonNegative}*");
    }

    [Fact]
    public void Execute_WithMaxPriceLessThanMinPrice_ThrowsValidationException()
    {
        var command = new CreatePriceRangeCommand(20m, 10m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{PriceRangeValidationMessages.MaxPriceGreaterThanOrEqualMinPrice}*");
    }
}
