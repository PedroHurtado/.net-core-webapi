namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class AddressCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Address.Create _create = fixture.Get<Address.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsAddress()
    {
        var command = new CreateAddressCommand(
            "Ctra. Murcia, 23",
            "La Puebla de Mula",
            "30193",
            "Murcia",
            "España",
            38.0389m,
            -1.4917m);

        var result = _create.Execute(command);

        result.Street.Should().Be("Ctra. Murcia, 23");
        result.City.Should().Be("La Puebla de Mula");
        result.PostalCode.Should().Be("30193");
        result.Region.Should().Be("Murcia");
        result.Country.Should().Be("España");
        result.Location.Latitude.Should().Be(38.0389m);
        result.FullAddress.Should().Be("Ctra. Murcia, 23, 30193 La Puebla de Mula, Murcia, España");
    }

    [Fact]
    public void Execute_WithEmptyStreet_ThrowsValidationException()
    {
        var command = new CreateAddressCommand("", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{AddressValidationMessages.StreetRequired}*");
    }

    [Fact]
    public void Execute_WithEmptyCity_ThrowsValidationException()
    {
        var command = new CreateAddressCommand("Ctra. Murcia, 23", "", "30193", "Murcia", "España", 38.0389m, -1.4917m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{AddressValidationMessages.CityRequired}*");
    }

    [Fact]
    public void Execute_WithEmptyPostalCode_ThrowsValidationException()
    {
        var command = new CreateAddressCommand("Ctra. Murcia, 23", "La Puebla de Mula", "", "Murcia", "España", 38.0389m, -1.4917m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{AddressValidationMessages.PostalCodeRequired}*");
    }

    [Fact]
    public void Execute_WithLatitudeOutOfRange_ThrowsValidationException()
    {
        var command = new CreateAddressCommand("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 91m, -1.4917m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LatitudeRange}*");
    }

    [Fact]
    public void Execute_WithLongitudeOutOfRange_ThrowsValidationException()
    {
        var command = new CreateAddressCommand("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -181m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{GeoPointValidationMessages.LongitudeRange}*");
    }
}
