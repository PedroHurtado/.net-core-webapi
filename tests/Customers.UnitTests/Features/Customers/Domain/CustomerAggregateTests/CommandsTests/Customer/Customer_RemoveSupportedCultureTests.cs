namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerRemoveSupportedCultureTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.RemoveSupportedCulture _removeSupportedCulture = fixture.Get<Customer.RemoveSupportedCulture>();

    [Fact]
    public void Execute_WithExistingCulture_RemovesCulture()
    {
        var customer = CreateValidCustomer()
            .WithSupportedCulture(new CultureCode("en-GB"));

        var result = _removeSupportedCulture.Execute(customer, new RemoveSupportedCultureCommand("en-GB"));

        result.SupportedCultures.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenCultureNotFound_ThrowsKeyNotFoundException()
    {
        var customer = CreateValidCustomer();

        var act = () => _removeSupportedCulture.Execute(customer, new RemoveSupportedCultureCommand("fr-FR"));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Culture 'fr-FR' not found*");
    }

    private static TestableCustomer CreateValidCustomer() =>
        new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "Espa\u00f1a", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678", new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "Espa\u00f1a", new GeoPoint(38.0389m, -1.4917m))));
}
