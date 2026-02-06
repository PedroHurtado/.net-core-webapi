namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerSetPriceRangeTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.SetPriceRange _setPriceRange = fixture.Get<Customer.SetPriceRange>();

    [Fact]
    public void Execute_WithValidCommand_SetsPriceRange()
    {
        var customer = CreateValidCustomer();

        var command = new SetPriceRangeCommand(10m, 30m);

        var result = _setPriceRange.Execute(customer, command);

        result.PriceRange.Should().NotBeNull();
        result.PriceRange!.MinPrice.Should().Be(10m);
        result.PriceRange.MaxPrice.Should().Be(30m);
        result.HasPriceRange.Should().BeTrue();
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
