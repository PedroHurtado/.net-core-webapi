namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateBillingInfoTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateBillingInfo.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateBillingInfo.Service _service;

    public UpdateBillingInfoTests(DomainFixture fixture)
    {
        _service = new UpdateBillingInfo.Service(
            fixture.Get<Customer.UpdateBillingInfo>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesBillingInfo()
    {
        var customer = new TestableCustomer(_tenantId)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Old SL", "A00000000",
                new Address("Old Street", "Old City", "00000", "Old Region", "Old Country", new GeoPoint(0m, 0m))));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateBillingInfo.Request(
            "Bar Juanjo SL",
            "B12345678",
            new UpdateBillingInfo.AddressRequest("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m));

        await _service.HandleAsync(request);

        customer.BillingInfo.BusinessName.Should().Be("Bar Juanjo SL");
        customer.BillingInfo.TaxId.Should().Be("B12345678");
    }
}
