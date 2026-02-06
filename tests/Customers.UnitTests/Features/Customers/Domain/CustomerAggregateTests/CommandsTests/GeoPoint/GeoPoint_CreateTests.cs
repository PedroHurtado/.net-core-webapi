namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class GeoPointCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly GeoPoint.Create _create = fixture.Get<GeoPoint.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsGeoPoint()
    {
        var command = new CreateGeoPointCommand(38.0389m, -1.4917m);

        var result = _create.Execute(command);

        result.Latitude.Should().Be(38.0389m);
        result.Longitude.Should().Be(-1.4917m);
    }

    [Fact]
    public void Execute_WithNorthPoleCoordinates_ReturnsGeoPoint()
    {
        var command = new CreateGeoPointCommand(90m, 0m);

        var result = _create.Execute(command);

        result.Latitude.Should().Be(90m);
        result.Longitude.Should().Be(0m);
    }

    [Fact]
    public void Execute_WithAntimeridianCoordinates_ReturnsGeoPoint()
    {
        var command = new CreateGeoPointCommand(0m, -180m);

        var result = _create.Execute(command);

        result.Latitude.Should().Be(0m);
        result.Longitude.Should().Be(-180m);
    }

    [Fact]
    public void Execute_WithLatitudeAboveRange_ThrowsValidationException()
    {
        var command = new CreateGeoPointCommand(91m, 0m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LatitudeRange}*");
    }

    [Fact]
    public void Execute_WithLatitudeBelowRange_ThrowsValidationException()
    {
        var command = new CreateGeoPointCommand(-91m, 0m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LatitudeRange}*");
    }

    [Fact]
    public void Execute_WithLongitudeAboveRange_ThrowsValidationException()
    {
        var command = new CreateGeoPointCommand(0m, 181m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LongitudeRange}*");
    }

    [Fact]
    public void Execute_WithLongitudeBelowRange_ThrowsValidationException()
    {
        var command = new CreateGeoPointCommand(0m, -181m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LongitudeRange}*");
    }
}
