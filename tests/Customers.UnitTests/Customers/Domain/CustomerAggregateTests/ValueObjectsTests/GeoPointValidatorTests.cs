namespace Customers.UnitTests.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class GeoPointValidatorTests
{
    private readonly GeoPointValidator _validator = new();

    [Fact]
    public void Validate_WithValidGeoPoint_ReturnsSuccess()
    {
        var geoPoint = new GeoPoint(38.0389m, -1.4917m);

        var result = _validator.Validate(geoPoint);

        result.IsValid.Should().BeTrue();
    }

    #region Latitude Validation

    [Theory]
    [InlineData(90)]
    [InlineData(0)]
    [InlineData(-90)]
    public void Latitude_WhenValid_ReturnsSuccess(decimal latitude)
    {
        var geoPoint = new GeoPoint(latitude, 0);

        var result = _validator.Validate(geoPoint);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    public void Latitude_WhenOutOfRange_ReturnsError(decimal latitude)
    {
        var geoPoint = new GeoPoint(latitude, 0);

        var result = _validator.Validate(geoPoint);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == GeoPointValidationMessages.LatitudeRange);
    }

    #endregion

    #region Longitude Validation

    [Theory]
    [InlineData(180)]
    [InlineData(0)]
    [InlineData(-180)]
    public void Longitude_WhenValid_ReturnsSuccess(decimal longitude)
    {
        var geoPoint = new GeoPoint(0, longitude);

        var result = _validator.Validate(geoPoint);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(181)]
    [InlineData(-181)]
    public void Longitude_WhenOutOfRange_ReturnsError(decimal longitude)
    {
        var geoPoint = new GeoPoint(0, longitude);

        var result = _validator.Validate(geoPoint);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == GeoPointValidationMessages.LongitudeRange);
    }

    #endregion
}
