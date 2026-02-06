namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateBillingInfoTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.UpdateBillingInfo _updateBillingInfo = fixture.Get<Customer.UpdateBillingInfo>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesBillingInfo()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateBillingInfoCommand(
            "Juanjo y María SL",
            "B87654321",
            new CreateAddressCommand("C/ Gran Vía, 1", "Murcia", "30001", "Murcia", "España", 37.9838m, -1.1280m));

        var result = _updateBillingInfo.Execute(customer, command);

        result.BillingInfo.BusinessName.Should().Be("Juanjo y María SL");
        result.BillingInfo.TaxId.Should().Be("B87654321");
        result.BillingInfo.BillingAddress.Street.Should().Be("C/ Gran Vía, 1");
        result.BillingInfo.BillingAddress.City.Should().Be("Murcia");
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
