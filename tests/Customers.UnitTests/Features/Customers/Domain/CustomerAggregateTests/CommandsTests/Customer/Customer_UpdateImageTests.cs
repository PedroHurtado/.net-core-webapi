namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerUpdateImageTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.UpdateImage _updateImage = fixture.Get<Customer.UpdateImage>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesImagePreservingUrl()
    {
        var imageId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(imageId, "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, false));

        var command = new UpdateImageCommand(imageId, "Fachada renovada", 2, false);

        var result = _updateImage.Execute(customer, command);

        var image = result.Images.First(i => i.Id == imageId);
        image.Url.Should().Be("https://cdn.fudie.com/images/fachada.jpg");
        image.AltText.Should().Be("Fachada renovada");
        image.DisplayOrder.Should().Be(2);
    }

    [Fact]
    public void Execute_PromoteToCover_DemotesExistingCover()
    {
        var coverImageId = Guid.NewGuid();
        var otherImageId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(coverImageId, "https://cdn.fudie.com/images/a.jpg", "A", 0, true))
            .WithImage(new CustomerImage(otherImageId, "https://cdn.fudie.com/images/b.jpg", "B", 1, false));

        var command = new UpdateImageCommand(otherImageId, "B promoted", 1, true);

        var result = _updateImage.Execute(customer, command);

        var imageA = result.Images.First(i => i.Id == coverImageId);
        imageA.IsCover.Should().BeFalse();
        var imageB = result.Images.First(i => i.Id == otherImageId);
        imageB.IsCover.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenImageNotFound_ThrowsKeyNotFoundException()
    {
        var customer = CreateValidCustomer();

        var command = new UpdateImageCommand(Guid.NewGuid(), "Alt", 0, false);

        var act = () => _updateImage.Execute(customer, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Image not found*");
    }

    private static TestableCustomer CreateValidCustomer() =>
        new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "Espa\u00f1a", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678", new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "Espa\u00f1a", new GeoPoint(38.0389m, -1.4917m))));
}
