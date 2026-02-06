namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateSocialLinkTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.UpdateSocialLink _updateSocialLink = fixture.Get<Customer.UpdateSocialLink>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesUrlPreservingPlatform()
    {
        var customer = CreateValidCustomer()
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo"));

        var command = new UpdateSocialLinkCommand("Facebook", "https://facebook.com/bardeljuanjo-nuevo");

        var result = _updateSocialLink.Execute(customer, command);

        var link = result.SocialLinks.First();
        link.Platform.Should().Be("Facebook");
        link.Url.Should().Be("https://facebook.com/bardeljuanjo-nuevo");
    }

    [Fact]
    public void Execute_WhenSocialLinkNotFound_ThrowsKeyNotFoundException()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateSocialLinkCommand("TripAdvisor", "https://tripadvisor.es/review");

        var act = () => _updateSocialLink.Execute(customer, command);

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
