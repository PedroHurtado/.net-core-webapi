namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Queries;

public class GetCustomerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<GetCustomer.IRepository> _repository = new();
    private readonly GetCustomer.Service _service;

    public GetCustomerTests()
    {
        _service = new GetCustomer.Service(_tenantId, _repository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ReturnsResponse()
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

        var response = await _service.HandleAsync();

        response.Name.Should().Be("El Bar del Juanjo");
        response.Slug.Should().Be("el-bar-del-juanjo");
    }
}
