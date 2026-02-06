namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerRemovePriceRangeTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.RemovePriceRange _removePriceRange = fixture.Get<Customer.RemovePriceRange>();

    [Fact]
    public void Execute_WithPriceRange_RemovesPriceRange()
    {
        var customer = CreateValidCustomer()
            .WithPriceRange(new PriceRange(10m, 30m));

        var result = _removePriceRange.Execute(customer);

        result.PriceRange.Should().BeNull();
        result.HasPriceRange.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithoutPriceRange_RemainsNull()
    {
        var customer = CreateValidCustomer();

        var result = _removePriceRange.Execute(customer);

        result.PriceRange.Should().BeNull();
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
