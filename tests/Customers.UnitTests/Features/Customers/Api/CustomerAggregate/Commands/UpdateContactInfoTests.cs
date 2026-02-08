namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateContactInfoTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateContactInfo.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateContactInfo.Service _service;

    public UpdateContactInfoTests(DomainFixture fixture)
    {
        _service = new UpdateContactInfo.Service(
            fixture.Get<Customer.UpdateContactInfo>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesContactInfo()
    {
        var customer = new TestableCustomer(_tenantId)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("000000000", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678",
                new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateContactInfo.Request("639079481", "info@bar.com", "https://bar.com");

        await _service.HandleAsync(request);

        customer.ContactInfo.Phone.Should().Be("639079481");
        customer.ContactInfo.Email.Should().Be("info@bar.com");
    }
}
