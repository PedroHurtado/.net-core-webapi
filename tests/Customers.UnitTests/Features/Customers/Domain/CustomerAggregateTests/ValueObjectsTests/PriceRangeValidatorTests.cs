namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class PriceRangeValidatorTests
{
    private readonly PriceRangeValidator _validator = new();

    [Fact]
    public void Validate_WithValidPriceRange_ReturnsSuccess()
    {
        var priceRange = new PriceRange(10, 20);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeTrue();
    }

    #region MinPrice Validation

    [Fact]
    public void MinPrice_WhenNegative_ReturnsError()
    {
        var priceRange = new PriceRange(-5, 20);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PriceRangeValidationMessages.MinPriceNonNegative);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void MinPrice_WhenValid_ReturnsSuccess(decimal minPrice)
    {
        var priceRange = new PriceRange(minPrice, 20);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region MaxPrice Validation

    [Fact]
    public void MaxPrice_WhenNegative_ReturnsError()
    {
        var priceRange = new PriceRange(0, -5);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PriceRangeValidationMessages.MaxPriceNonNegative);
    }

    [Fact]
    public void MaxPrice_WhenLessThanMinPrice_ReturnsError()
    {
        var priceRange = new PriceRange(20, 10);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PriceRangeValidationMessages.MaxPriceGreaterThanOrEqualMinPrice);
    }

    [Fact]
    public void MaxPrice_WhenEqualToMinPrice_ReturnsSuccess()
    {
        var priceRange = new PriceRange(15, 15);

        var result = _validator.Validate(priceRange);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
