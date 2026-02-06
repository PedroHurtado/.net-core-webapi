namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class GeoPointTests
{
    #region Latitude

    [Theory]
    [InlineData(38.0389)]
    [InlineData(90)]
    [InlineData(-90)]
    [InlineData(0)]
    public void Latitude_SetsCorrectValue(decimal latitude)
    {
        var geoPoint = new GeoPoint(latitude, 0);

        geoPoint.Latitude.Should().Be(latitude);
    }

    #endregion

    #region Longitude

    [Theory]
    [InlineData(-1.4917)]
    [InlineData(180)]
    [InlineData(-180)]
    [InlineData(0)]
    public void Longitude_SetsCorrectValue(decimal longitude)
    {
        var geoPoint = new GeoPoint(0, longitude);

        geoPoint.Longitude.Should().Be(longitude);
    }

    #endregion
}
