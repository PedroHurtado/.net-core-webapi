namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateAddressTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.UpdateAddress _updateAddress = fixture.Get<Customer.UpdateAddress>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesAddress()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateAddressCommand(
            "Ctra. Murcia, 25",
            "La Puebla de Mula",
            "30193",
            "Murcia",
            "España",
            38.0390m,
            -1.4918m);

        var result = _updateAddress.Execute(customer, command);

        result.Address.Street.Should().Be("Ctra. Murcia, 25");
        result.Address.Location.Latitude.Should().Be(38.0390m);
        result.Address.Location.Longitude.Should().Be(-1.4918m);
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
