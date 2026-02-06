namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerDeactivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.Deactivate _deactivate = fixture.Get<Customer.Deactivate>();

    [Fact]
    public void Execute_WhenActive_DeactivatesCustomer()
    {
        var customer = CreateValidCustomer()
            .WithIsActive(true);

        var result = _deactivate.Execute(customer);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var customer = CreateValidCustomer();

        var act = () => _deactivate.Execute(customer);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Customer is already inactive*");
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
