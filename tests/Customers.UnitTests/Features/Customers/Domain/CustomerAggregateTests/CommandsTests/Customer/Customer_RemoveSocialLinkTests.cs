namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerRemoveSocialLinkTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.RemoveSocialLink _removeSocialLink = fixture.Get<Customer.RemoveSocialLink>();

    [Fact]
    public void Execute_WithExistingPlatform_RemovesSocialLink()
    {
        var customer = CreateValidCustomer()
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo"));

        var result = _removeSocialLink.Execute(customer, new RemoveSocialLinkCommand("Facebook"));

        result.SocialLinks.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenSocialLinkNotFound_ThrowsKeyNotFoundException()
    {
        var customer = CreateValidCustomer();

        var act = () => _removeSocialLink.Execute(customer, new RemoveSocialLinkCommand("TripAdvisor"));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Social link for 'TripAdvisor' not found*");
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
