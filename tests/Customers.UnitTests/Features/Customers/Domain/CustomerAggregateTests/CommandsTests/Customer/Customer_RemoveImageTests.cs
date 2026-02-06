namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerRemoveImageTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.RemoveImage _removeImage = fixture.Get<Customer.RemoveImage>();

    [Fact]
    public void Execute_WithMultipleImages_RemovesImage()
    {
        var imageToRemoveId = Guid.NewGuid();
        var otherImageId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(imageToRemoveId, "https://cdn.fudie.com/images/a.jpg", "A", 0, false))
            .WithImage(new CustomerImage(otherImageId, "https://cdn.fudie.com/images/b.jpg", "B", 1, false));

        var result = _removeImage.Execute(customer, new RemoveImageCommand(imageToRemoveId));

        result.Images.Should().HaveCount(1);
        result.Images.First().Id.Should().Be(otherImageId);
    }

    [Fact]
    public void Execute_WithLastImage_HasImagesBecomesFalse()
    {
        var imageId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(imageId, "https://cdn.fudie.com/images/a.jpg", "A", 0, false));

        var result = _removeImage.Execute(customer, new RemoveImageCommand(imageId));

        result.Images.Should().BeEmpty();
        result.HasImages.Should().BeFalse();
    }

    [Fact]
    public void Execute_RemoveCover_CoverImageFallsToDisplayOrder()
    {
        var coverId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var customer = CreateValidCustomer()
            .WithImage(new CustomerImage(coverId, "https://cdn.fudie.com/images/a.jpg", "A", 0, true))
            .WithImage(new CustomerImage(otherId, "https://cdn.fudie.com/images/b.jpg", "B", 1, false));

        var result = _removeImage.Execute(customer, new RemoveImageCommand(coverId));

        result.CoverImage.Should().NotBeNull();
        result.CoverImage!.Id.Should().Be(otherId);
    }

    [Fact]
    public void Execute_WhenImageNotFound_ThrowsKeyNotFoundException()
    {
        var customer = CreateValidCustomer();

        var act = () => _removeImage.Execute(customer, new RemoveImageCommand(Guid.NewGuid()));

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
