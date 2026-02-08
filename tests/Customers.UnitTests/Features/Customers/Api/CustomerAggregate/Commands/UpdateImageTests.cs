namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateImageTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<UpdateImage.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateImage.Service _service;

    public UpdateImageTests(DomainFixture fixture)
    {
        _service = new UpdateImage.Service(
            fixture.Get<Customer.UpdateImage>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesImage()
    {
        var imageId = Guid.NewGuid();
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
            .WithImage(new CustomerImage(imageId, "https://images.bar.com/logo.jpg", "Logo", 0, false));

        _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(customer);

        var request = new UpdateImage.Request("Logo actualizado", 1, true);

        await _service.HandleAsync(imageId, request);

        var image = customer.Images.Single();
        image.AltText.Should().Be("Logo actualizado");
        image.DisplayOrder.Should().Be(1);
        image.IsCover.Should().BeTrue();
    }
}
