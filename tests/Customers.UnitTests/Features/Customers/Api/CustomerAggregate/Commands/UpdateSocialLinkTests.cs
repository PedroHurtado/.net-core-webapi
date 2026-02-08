namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateSocialLinkTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateSocialLink.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateSocialLink.Service _service;

    public UpdateSocialLinkTests(DomainFixture fixture)
    {
        _service = new UpdateSocialLink.Service(
            fixture.Get<Customer.UpdateSocialLink>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesSocialLink()
    {
        var customer = new TestableCustomer(_tenantId)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678",
                new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))))
            .WithSocialLink(new SocialLink("Instagram", "https://instagram.com/old"));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateSocialLink.Request("https://instagram.com/barjuanjo");

        await _service.HandleAsync("Instagram", request);

        customer.SocialLinks.Single().Url.Should().Be("https://instagram.com/barjuanjo");
    }
}
