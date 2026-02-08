namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateAddressTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateAddress.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateAddress.Service _service;

    public UpdateAddressTests(DomainFixture fixture)
    {
        _service = new UpdateAddress.Service(
            fixture.Get<Customer.UpdateAddress>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesAddress()
    {
        var customer = new TestableCustomer(_tenantId)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Old Street", "Old City", "00000", "Old Region", "Old Country", new GeoPoint(0m, 0m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678",
                new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateAddress.Request(
            "Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m);

        await _service.HandleAsync(request);

        customer.Address.Street.Should().Be("Ctra. Murcia, 23");
        customer.Address.Location.Latitude.Should().Be(38.0389m);
    }
}
