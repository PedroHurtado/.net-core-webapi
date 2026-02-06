namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests;

public class CustomerTests
{
    [Fact]
    public void Customer_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m));
        var contactInfo = new ContactInfo("639079481", "juanjo@example.com", "https://facebook.com/elbardeljuanjo");
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", address);
        var priceRange = new PriceRange(10m, 30m);

        // Act
        var customer = new TestableCustomer(id)
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithDescription("Bar de tapas")
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithIsActive(false)
            .WithAddress(address)
            .WithContactInfo(contactInfo)
            .WithBillingInfo(billingInfo)
            .WithPriceRange(priceRange);

        // Assert
        customer.Id.Should().Be(id);
        customer.Name.Should().Be("El Bar del Juanjo");
        customer.Slug.Should().Be("el-bar-del-juanjo");
        customer.Description.Should().Be("Bar de tapas");
        customer.LogoUrl.Should().Be("https://cdn.fudie.com/logo.jpg");
        customer.EstablishmentType.Should().Be("Bar");
        customer.DefaultCulture.Should().Be("es-ES");
        customer.TimeZoneId.Should().Be("Europe/Madrid");
        customer.IsActive.Should().BeFalse();
        customer.Address.Should().Be(address);
        customer.ContactInfo.Should().Be(contactInfo);
        customer.BillingInfo.Should().Be(billingInfo);
        customer.PriceRange.Should().Be(priceRange);
    }

    // --- HasPriceRange ---

    [Fact]
    public void HasPriceRange_WithPriceRangeSet_ShouldReturnTrue()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithPriceRange(new PriceRange(10m, 30m));

        // Assert
        customer.HasPriceRange.Should().BeTrue();
    }

    [Fact]
    public void HasPriceRange_WithNullPriceRange_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithPriceRange(null);

        // Assert
        customer.HasPriceRange.Should().BeFalse();
    }

    // --- HasLogo ---

    [Fact]
    public void HasLogo_WithLogoUrl_ShouldReturnTrue()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg");

        // Assert
        customer.HasLogo.Should().BeTrue();
    }

    [Fact]
    public void HasLogo_WithNullLogoUrl_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl(null);

        // Assert
        customer.HasLogo.Should().BeFalse();
    }

    [Fact]
    public void HasLogo_WithEmptyLogoUrl_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl(string.Empty);

        // Assert
        customer.HasLogo.Should().BeFalse();
    }

    // --- HasImages ---

    [Fact]
    public void HasImages_WithImages_ShouldReturnTrue()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false));

        // Assert
        customer.HasImages.Should().BeTrue();
    }

    [Fact]
    public void HasImages_WithNoImages_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid());

        // Assert
        customer.HasImages.Should().BeFalse();
    }

    // --- CoverImage ---

    [Fact]
    public void CoverImage_WithCoverFlagSet_ShouldReturnCoverImage()
    {
        // Arrange
        var cover = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/cover.jpg", null, 1, true);
        var other = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/other.jpg", null, 0, false);
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithImage(other)
            .WithImage(cover);

        // Assert
        customer.CoverImage.Should().Be(cover);
    }

    [Fact]
    public void CoverImage_WithNoCoverFlag_ShouldReturnFirstByDisplayOrder()
    {
        // Arrange
        var img1 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img1.jpg", null, 2, false);
        var img2 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img2.jpg", null, 0, false);
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithImage(img1)
            .WithImage(img2);

        // Assert
        customer.CoverImage.Should().Be(img2);
    }

    [Fact]
    public void CoverImage_WithNoImages_ShouldReturnNull()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid());

        // Assert
        customer.CoverImage.Should().BeNull();
    }

    // --- IsProfileComplete ---

    [Fact]
    public void IsProfileComplete_WithAllRequiredData_ShouldReturnTrue()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithDescription("Descripción")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false))
            .WithCuisineType("Española");

        // Assert
        customer.IsProfileComplete.Should().BeTrue();
    }

    [Fact]
    public void IsProfileComplete_WithoutLogo_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithDescription("Descripción")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false))
            .WithCuisineType("Española");

        // Assert
        customer.IsProfileComplete.Should().BeFalse();
    }

    [Fact]
    public void IsProfileComplete_WithoutImages_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithDescription("Descripción")
            .WithCuisineType("Española");

        // Assert
        customer.IsProfileComplete.Should().BeFalse();
    }

    [Fact]
    public void IsProfileComplete_WithoutDescription_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false))
            .WithCuisineType("Española");

        // Assert
        customer.IsProfileComplete.Should().BeFalse();
    }

    [Fact]
    public void IsProfileComplete_WithoutCuisineTypes_ShouldReturnFalse()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithDescription("Descripción")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false));

        // Assert
        customer.IsProfileComplete.Should().BeFalse();
    }

    // --- Images collection ---

    [Fact]
    public void Images_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img.jpg", null, 0, false));

        // Assert
        customer.Images.Should().BeAssignableTo<IReadOnlyCollection<CustomerImage>>();
        customer.Images.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleImages_ShouldContainAllImages()
    {
        // Arrange
        var img1 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img1.jpg", null, 0, false);
        var img2 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img2.jpg", null, 1, false);
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithImage(img1)
            .WithImage(img2);

        // Assert
        customer.Images.Should().HaveCount(2);
        customer.Images.Should().Contain(x => x.Url == "https://cdn.fudie.com/img1.jpg");
        customer.Images.Should().Contain(x => x.Url == "https://cdn.fudie.com/img2.jpg");
    }

    // --- CuisineTypes collection ---

    [Fact]
    public void CuisineTypes_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithCuisineType("Española");

        // Assert
        customer.CuisineTypes.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        customer.CuisineTypes.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleCuisineTypes_ShouldContainAllCuisineTypes()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithCuisineType("Española")
            .WithCuisineType("Mediterránea");

        // Assert
        customer.CuisineTypes.Should().HaveCount(2);
        customer.CuisineTypes.Should().Contain("Española");
        customer.CuisineTypes.Should().Contain("Mediterránea");
    }

    // --- ServiceAmenities collection ---

    [Fact]
    public void ServiceAmenities_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithServiceAmenity("Terraza");

        // Assert
        customer.ServiceAmenities.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        customer.ServiceAmenities.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleServiceAmenities_ShouldContainAllServiceAmenities()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithServiceAmenity("Terraza")
            .WithServiceAmenity("Pet friendly");

        // Assert
        customer.ServiceAmenities.Should().HaveCount(2);
        customer.ServiceAmenities.Should().Contain("Terraza");
        customer.ServiceAmenities.Should().Contain("Pet friendly");
    }

    // --- DietaryOptions collection ---

    [Fact]
    public void DietaryOptions_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithDietaryOption("Celíacos");

        // Assert
        customer.DietaryOptions.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        customer.DietaryOptions.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleDietaryOptions_ShouldContainAllDietaryOptions()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithDietaryOption("Celíacos")
            .WithDietaryOption("Vegano");

        // Assert
        customer.DietaryOptions.Should().HaveCount(2);
        customer.DietaryOptions.Should().Contain("Celíacos");
        customer.DietaryOptions.Should().Contain("Vegano");
    }

    // --- SupportedCultures collection ---

    [Fact]
    public void SupportedCultures_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithSupportedCulture(new CultureCode("en-GB"));

        // Assert
        customer.SupportedCultures.Should().BeAssignableTo<IReadOnlyCollection<CultureCode>>();
        customer.SupportedCultures.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleSupportedCultures_ShouldContainAllSupportedCultures()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithSupportedCulture(new CultureCode("en-GB"))
            .WithSupportedCulture(new CultureCode("fr-FR"));

        // Assert
        customer.SupportedCultures.Should().HaveCount(2);
        customer.SupportedCultures.Should().Contain(x => x.Code == "en-GB");
        customer.SupportedCultures.Should().Contain(x => x.Code == "fr-FR");
    }

    // --- SocialLinks collection ---

    [Fact]
    public void SocialLinks_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/test"));

        // Assert
        customer.SocialLinks.Should().BeAssignableTo<IReadOnlyCollection<SocialLink>>();
        customer.SocialLinks.Should().ContainSingle();
    }

    [Fact]
    public void Customer_WithMultipleSocialLinks_ShouldContainAllSocialLinks()
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/test"))
            .WithSocialLink(new SocialLink("Instagram", "https://instagram.com/test"));

        // Assert
        customer.SocialLinks.Should().HaveCount(2);
        customer.SocialLinks.Should().Contain(x => x.Platform == "Facebook");
        customer.SocialLinks.Should().Contain(x => x.Platform == "Instagram");
    }

    // --- IsActive ---

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Customer_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var customer = new TestableCustomer(Guid.NewGuid())
            .WithIsActive(value);

        // Assert
        customer.IsActive.Should().Be(value);
    }
}
