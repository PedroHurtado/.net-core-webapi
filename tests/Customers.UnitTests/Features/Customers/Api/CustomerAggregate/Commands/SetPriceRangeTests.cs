namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class SetPriceRangeTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<SetPriceRange.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SetPriceRange.Service _service;

    public SetPriceRangeTests(DomainFixture fixture)
    {
        _service = new SetPriceRange.Service(
            fixture.Get<Customer.SetPriceRange>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_SetsPriceRange()
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
                new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));
        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new SetPriceRange.Request(10.00m, 50.00m);

        await _service.HandleAsync(request);

        customer.PriceRange.Should().NotBeNull();
        customer.PriceRange!.MinPrice.Should().Be(10.00m);
        customer.PriceRange.MaxPrice.Should().Be(50.00m);
    }
}
