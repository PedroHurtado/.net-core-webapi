namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.Create _create = fixture.Get<Customer.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsCustomer()
    {
        var command = new CreateCustomerCommand(
            "El Bar del Juanjo",
            "el-bar-del-juanjo",
            "Bar de tapas",
            "Bar",
            "es-ES",
            "Europe/Madrid",
            new CreateAddressCommand("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m),
            new CreateContactInfoCommand("639079481", null, null),
            new CreateBillingInfoCommand("Bar Juanjo SL", "B12345678",
                new CreateAddressCommand("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m)));

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("El Bar del Juanjo");
        result.Slug.Should().Be("el-bar-del-juanjo");
        result.Description.Should().Be("Bar de tapas");
        result.EstablishmentType.Should().Be("Bar");
        result.DefaultCulture.Should().Be("es-ES");
        result.TimeZoneId.Should().Be("Europe/Madrid");
        result.IsActive.Should().BeFalse();
        result.PriceRange.Should().BeNull();
        result.LogoUrl.Should().BeNull();
        result.Address.Street.Should().Be("Ctra. Murcia, 23");
        result.Address.Location.Latitude.Should().Be(38.0389m);
        result.ContactInfo.Phone.Should().Be("639079481");
        result.BillingInfo.BusinessName.Should().Be("Bar Juanjo SL");
        result.Images.Should().BeEmpty();
        result.CuisineTypes.Should().BeEmpty();
        result.ServiceAmenities.Should().BeEmpty();
        result.DietaryOptions.Should().BeEmpty();
        result.SupportedCultures.Should().BeEmpty();
        result.SocialLinks.Should().BeEmpty();
    }
}
