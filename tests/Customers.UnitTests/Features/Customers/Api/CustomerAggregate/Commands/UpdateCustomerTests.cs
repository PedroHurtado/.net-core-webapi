namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateCustomerTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateCustomer.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateCustomer.Service _service;

    public UpdateCustomerTests(DomainFixture fixture)
    {
        _service = new UpdateCustomer.Service(
            fixture.Get<Customer.Update>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    private TestableCustomer CreateExistingCustomer() => new TestableCustomer(_tenantId)
        .WithName("El Bar del Juanjo")
        .WithSlug("el-bar-del-juanjo")
        .WithEstablishmentType("Bar")
        .WithDefaultCulture("es-ES")
        .WithTimeZoneId("Europe/Madrid")
        .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
        .WithContactInfo(new ContactInfo("639079481", null, null))
        .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678",
            new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesCustomer()
    {
        var customer = CreateExistingCustomer();
        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateCustomer.Request(
            "Nuevo Nombre",
            "el-bar-del-juanjo",
            "Nueva descripción",
            null,
            "Restaurante",
            "es-ES",
            "Europe/Madrid",
            ["Española"],
            ["WiFi"],
            ["Vegano"]);

        await _service.HandleAsync(request);

        customer.Name.Should().Be("Nuevo Nombre");
    }

    [Fact]
    public async Task HandleAsync_WhenSlugChangedAndExists_ThrowsConflictException()
    {
        var customer = CreateExistingCustomer();
        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);
        _repository.Setup(r => r.ExistsBySlug("nuevo-slug")).ReturnsAsync(true);

        var request = new UpdateCustomer.Request(
            "Nuevo Nombre",
            "nuevo-slug",
            null,
            null,
            "Bar",
            "es-ES",
            "Europe/Madrid",
            [],
            [],
            []);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task HandleAsync_WhenSlugUnchanged_DoesNotCheckExistsBySlug()
    {
        var customer = CreateExistingCustomer();
        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateCustomer.Request(
            "Nuevo Nombre",
            "el-bar-del-juanjo",
            null,
            null,
            "Bar",
            "es-ES",
            "Europe/Madrid",
            [],
            [],
            []);

        await _service.HandleAsync(request);

        _repository.Verify(r => r.ExistsBySlug(It.IsAny<string>()), Times.Never);
    }
}
