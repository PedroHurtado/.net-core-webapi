namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class ActivateCustomerTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<ActivateCustomer.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivateCustomer.Service _service;

    public ActivateCustomerTests(DomainFixture fixture)
    {
        _service = new ActivateCustomer.Service(
            fixture.Get<Customer.Activate>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCustomer_ReturnsResponseWithIsActiveTrue()
    {
        var customer = new TestableCustomer(_tenantId)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithDescription("Bar de tapas")
            .WithLogoUrl("https://images.bar.com/logo.jpg")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithIsActive(false)
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678",
                new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))))
            .WithCuisineType("Española")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://images.bar.com/foto.jpg", "Foto", 0, true));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var response = await _service.HandleAsync();

        response.IsActive.Should().BeTrue();
    }
}
