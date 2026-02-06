namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerAddSupportedCultureTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.AddSupportedCulture _addSupportedCulture = fixture.Get<Customer.AddSupportedCulture>();

    [Fact]
    public void Execute_WithValidCommand_AddsCulture()
    {
        var customer = CreateValidCustomer();

        var command = new AddSupportedCultureCommand("en-GB");

        var result = _addSupportedCulture.Execute(customer, command);

        result.SupportedCultures.Should().HaveCount(1);
        result.SupportedCultures.First().Code.Should().Be("en-GB");
    }

    [Fact]
    public void Execute_WhenCultureDuplicated_ThrowsConflictException()
    {
        var customer = CreateValidCustomer()
            .WithSupportedCulture(new CultureCode("en-GB"));

        var command = new AddSupportedCultureCommand("en-GB");

        var act = () => _addSupportedCulture.Execute(customer, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Culture 'en-GB' is already supported*");
    }

    [Fact]
    public void Execute_WhenCultureIsDefault_ThrowsConflictException()
    {
        var customer = CreateValidCustomer();

        var command = new AddSupportedCultureCommand("es-ES");

        var act = () => _addSupportedCulture.Execute(customer, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Culture 'es-ES' is already the default culture*");
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
