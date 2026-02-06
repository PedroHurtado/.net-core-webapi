namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateContactInfoTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.UpdateContactInfo _updateContactInfo = fixture.Get<Customer.UpdateContactInfo>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesContactInfo()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateContactInfoCommand("639079482", "juanjo@bar.com", "https://elbardeljuanjo.com");

        var result = _updateContactInfo.Execute(customer, command);

        result.ContactInfo.Phone.Should().Be("639079482");
        result.ContactInfo.Email.Should().Be("juanjo@bar.com");
        result.ContactInfo.WebsiteUrl.Should().Be("https://elbardeljuanjo.com");
    }

    [Fact]
    public void Execute_WithNullOptionals_UpdatesContactInfoPhoneOnly()
    {
        var customer = CreateValidCustomer()
            .WithContactInfo(new ContactInfo("639079481", "old@email.com", "https://old.com"));

        var command = new UpdateContactInfoCommand("639079482", null, null);

        var result = _updateContactInfo.Execute(customer, command);

        result.ContactInfo.Phone.Should().Be("639079482");
        result.ContactInfo.Email.Should().BeNull();
        result.ContactInfo.WebsiteUrl.Should().BeNull();
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
