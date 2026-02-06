namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class PriceRangeTests
{
    #region MinPrice

    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(15)]
    public void MinPrice_SetsCorrectValue(decimal minPrice)
    {
        var priceRange = new PriceRange(minPrice, 100);

        priceRange.MinPrice.Should().Be(minPrice);
    }

    #endregion

    #region MaxPrice

    [Theory]
    [InlineData(20)]
    [InlineData(15)]
    [InlineData(100)]
    public void MaxPrice_SetsCorrectValue(decimal maxPrice)
    {
        var priceRange = new PriceRange(0, maxPrice);

        priceRange.MaxPrice.Should().Be(maxPrice);
    }

    #endregion
}
