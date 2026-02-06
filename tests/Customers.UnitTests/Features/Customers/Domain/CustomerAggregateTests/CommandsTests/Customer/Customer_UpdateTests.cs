namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.Update _update = fixture.Get<Customer.Update>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesCustomer()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateCustomerCommand(
            "El Bar del Juanjo y la María",
            "bar-juanjo-la-maria",
            "Nueva descripción",
            "https://cdn.fudie.com/logo.jpg",
            "Restaurante",
            "en-GB",
            "Europe/London",
            ["Española", "Mediterránea"],
            ["Terraza"],
            ["Vegano"]);

        var result = _update.Execute(customer, command);

        result.Name.Should().Be("El Bar del Juanjo y la María");
        result.Slug.Should().Be("bar-juanjo-la-maria");
        result.Description.Should().Be("Nueva descripción");
        result.LogoUrl.Should().Be("https://cdn.fudie.com/logo.jpg");
        result.EstablishmentType.Should().Be("Restaurante");
        result.DefaultCulture.Should().Be("en-GB");
        result.TimeZoneId.Should().Be("Europe/London");
    }

    [Fact]
    public void Execute_WithValidCommand_ReplacesCollections()
    {
        var customer = CreateValidCustomer()
            .WithCuisineType("Italiana")
            .WithCuisineType("Japonesa")
            .WithServiceAmenity("WiFi")
            .WithDietaryOption("Celíacos");

        var command = new UpdateCustomerCommand(
            "El Bar del Juanjo",
            "el-bar-del-juanjo",
            null,
            null,
            "Bar",
            "es-ES",
            "Europe/Madrid",
            ["Española", "Tapas"],
            ["Terraza", "Pet friendly"],
            ["Vegano"]);

        var result = _update.Execute(customer, command);

        result.CuisineTypes.Should().BeEquivalentTo(["Española", "Tapas"]);
        result.ServiceAmenities.Should().BeEquivalentTo(["Terraza", "Pet friendly"]);
        result.DietaryOptions.Should().BeEquivalentTo(["Vegano"]);
    }

    private static TestableCustomer CreateValidCustomer() =>
        new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678", new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));
}
