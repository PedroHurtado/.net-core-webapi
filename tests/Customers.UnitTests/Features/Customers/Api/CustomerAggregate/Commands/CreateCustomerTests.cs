namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class CreateCustomerTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<CreateCustomer.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateCustomer.Service _service;

    public CreateCustomerTests(DomainFixture fixture)
    {
        _service = new CreateCustomer.Service(
            fixture.Get<Customer.Create>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    private static CreateCustomer.Request CreateValidRequest(string slug = "el-bar-del-juanjo") =>
        new(
            "El Bar del Juanjo",
            slug,
            "Bar de tapas",
            "Bar",
            "es-ES",
            "Europe/Madrid",
            new CreateCustomer.CreateAddressRequest("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m),
            new CreateCustomer.CreateContactInfoRequest("639079481", null, null),
            new CreateCustomer.CreateBillingInfoRequest("Bar Juanjo SL", "B12345678",
                new CreateCustomer.CreateAddressRequest("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m)));

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Id.Should().Be(_tenantId);
        response.Name.Should().Be("El Bar del Juanjo");
    }

    [Fact]
    public async Task HandleAsync_WhenSlugExists_ThrowsConflictException()
    {
        _repository.Setup(r => r.ExistsBySlug("existing-slug")).ReturnsAsync(true);

        var request = CreateValidRequest(slug: "existing-slug");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
