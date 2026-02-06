namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerAddSocialLinkTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.AddSocialLink _addSocialLink = fixture.Get<Customer.AddSocialLink>();

    [Fact]
    public void Execute_WithValidCommand_AddsSocialLink()
    {
        var customer = CreateValidCustomer();

        var command = new AddSocialLinkCommand("Facebook", "https://facebook.com/elbardeljuanjo");

        var result = _addSocialLink.Execute(customer, command);

        result.SocialLinks.Should().HaveCount(1);
        result.SocialLinks.First().Platform.Should().Be("Facebook");
        result.SocialLinks.First().Url.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    [Fact]
    public void Execute_WhenPlatformDuplicated_ThrowsConflictException()
    {
        var customer = CreateValidCustomer()
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo"));

        var command = new AddSocialLinkCommand("Facebook", "https://facebook.com/otro");

        var act = () => _addSocialLink.Execute(customer, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Social link for 'Facebook' already exists*");
    }

    [Fact]
    public void Execute_WhenPlatformDuplicatedCaseInsensitive_ThrowsConflictException()
    {
        var customer = CreateValidCustomer()
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo"));

        var command = new AddSocialLinkCommand("facebook", "https://facebook.com/otro");

        var act = () => _addSocialLink.Execute(customer, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Social link for 'facebook' already exists*");
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
