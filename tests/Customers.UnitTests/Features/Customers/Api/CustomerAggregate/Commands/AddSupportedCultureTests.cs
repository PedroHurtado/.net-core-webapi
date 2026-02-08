namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class AddSupportedCultureTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<AddSupportedCulture.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AddSupportedCulture.Service _service;

    public AddSupportedCultureTests(DomainFixture fixture)
    {
        _service = new AddSupportedCulture.Service(
            fixture.Get<Customer.AddSupportedCulture>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponseWithCulture()
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

        var request = new AddSupportedCulture.Request("en-GB");

        var response = await _service.HandleAsync(request);

        response.SupportedCultures.Should().HaveCount(1);
        response.SupportedCultures.First().Code.Should().Be("en-GB");
    }
}
