namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerAddImageTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.AddImage _addImage = fixture.Get<Customer.AddImage>();

    [Fact]
    public void Execute_WithFirstImage_AddsImageAndHasImagesTrue()
    {
        var customer = CreateValidCustomer();

        var command = new AddImageCommand("https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true);

        var result = _addImage.Execute(customer, command);

        result.Images.Should().HaveCount(1);
        result.HasImages.Should().BeTrue();
        result.CoverImage.Should().NotBeNull();
        result.CoverImage!.Url.Should().Be("https://cdn.fudie.com/images/fachada.jpg");
    }

    [Fact]
    public void Execute_WithCover_DemotesExistingCover()
    {
        var existingCoverId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(existingCoverId, "https://cdn.fudie.com/images/old.jpg", "Old", 0, true));

        var command = new AddImageCommand("https://cdn.fudie.com/images/nueva-fachada.jpg", "Nueva fachada", 1, true);

        var result = _addImage.Execute(customer, command);

        result.Images.Should().HaveCount(2);
        var oldImage = result.Images.First(i => i.Id == existingCoverId);
        oldImage.IsCover.Should().BeFalse();
        var newImage = result.Images.First(i => i.Url == "https://cdn.fudie.com/images/nueva-fachada.jpg");
        newImage.IsCover.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithDuplicateUrl_ThrowsConflictException()
    {
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true));

        var command = new AddImageCommand("https://cdn.fudie.com/images/fachada.jpg");

        var act = () => _addImage.Execute(customer, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Image with this URL already exists*");
    }

    private static TestableCustomer CreateValidCustomer() =>
        new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678", new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));
}
