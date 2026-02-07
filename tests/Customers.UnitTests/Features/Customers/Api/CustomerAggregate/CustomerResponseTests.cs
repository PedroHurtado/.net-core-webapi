namespace Customers.UnitTests.Features.Customers.Api.CustomerAggregate;

public class CustomerResponseTests
{
    #region CustomerResponse.Map

    [Fact]
    public void CustomerResponse_Map_MapsAllProperties()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", location);
        var contactInfo = new ContactInfo("639079481", "juanjo@example.com", "https://facebook.com/elbardeljuanjo");
        var billingAddress = new Address("C/ Gran Vía, 1", "Murcia", "30001", "Murcia", "España", location);
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", billingAddress);
        var priceRange = new PriceRange(10m, 30m);
        var image = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true);
        var culture = new CultureCode("en-GB");
        var socialLink = new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo");

        var customer = new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithDescription("Bar de tapas")
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithIsActive(true)
            .WithAddress(address)
            .WithContactInfo(contactInfo)
            .WithBillingInfo(billingInfo)
            .WithPriceRange(priceRange)
            .WithImage(image)
            .WithCuisineType("Española")
            .WithServiceAmenity("Terraza")
            .WithDietaryOption("Vegano")
            .WithSupportedCulture(culture)
            .WithSocialLink(socialLink);

        var response = CustomerResponse.Map(customer);

        response.Id.Should().Be(customer.Id);
        response.Name.Should().Be("El Bar del Juanjo");
        response.Slug.Should().Be("el-bar-del-juanjo");
        response.Description.Should().Be("Bar de tapas");
        response.LogoUrl.Should().Be("https://cdn.fudie.com/logo.jpg");
        response.EstablishmentType.Should().Be("Bar");
        response.DefaultCulture.Should().Be("es-ES");
        response.TimeZoneId.Should().Be("Europe/Madrid");
        response.IsActive.Should().BeTrue();
        response.HasPriceRange.Should().BeTrue();
        response.HasLogo.Should().BeTrue();
        response.HasImages.Should().BeTrue();
        response.IsProfileComplete.Should().BeTrue();
        response.PriceRange.Should().NotBeNull();
        response.CoverImage.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.CuisineTypes.Should().HaveCount(1);
        response.ServiceAmenities.Should().HaveCount(1);
        response.DietaryOptions.Should().HaveCount(1);
        response.SupportedCultures.Should().HaveCount(1);
        response.SocialLinks.Should().HaveCount(1);
    }

    [Fact]
    public void CustomerResponse_Map_WithNullOptionalFields_MapsNullValues()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", location);
        var contactInfo = new ContactInfo("639079481", null, null);
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", address);

        var customer = new TestableCustomer(Guid.NewGuid())
            .WithName("Test")
            .WithSlug("test")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(address)
            .WithContactInfo(contactInfo)
            .WithBillingInfo(billingInfo);

        var response = CustomerResponse.Map(customer);

        response.Description.Should().BeNull();
        response.LogoUrl.Should().BeNull();
        response.PriceRange.Should().BeNull();
        response.CoverImage.Should().BeNull();
        response.HasPriceRange.Should().BeFalse();
        response.HasLogo.Should().BeFalse();
        response.HasImages.Should().BeFalse();
        response.IsProfileComplete.Should().BeFalse();
    }

    [Fact]
    public void CustomerResponse_Map_WithCollections_MapsCollections()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", location);
        var contactInfo = new ContactInfo("639079481", null, null);
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", address);
        var image1 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img1.jpg", "Img 1", 0, true);
        var image2 = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/img2.jpg", null, 1, false);

        var customer = new TestableCustomer(Guid.NewGuid())
            .WithName("Test")
            .WithSlug("test")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(address)
            .WithContactInfo(contactInfo)
            .WithBillingInfo(billingInfo)
            .WithImage(image1)
            .WithImage(image2)
            .WithCuisineType("Española")
            .WithCuisineType("Mediterránea")
            .WithSupportedCulture(new CultureCode("en-GB"))
            .WithSupportedCulture(new CultureCode("fr-FR"))
            .WithSocialLink(new SocialLink("Facebook", "https://facebook.com/test"))
            .WithSocialLink(new SocialLink("Instagram", "https://instagram.com/test"));

        var response = CustomerResponse.Map(customer);

        response.Images.Should().HaveCount(2);
        response.CuisineTypes.Should().HaveCount(2);
        response.SupportedCultures.Should().HaveCount(2);
        response.SocialLinks.Should().HaveCount(2);
        response.CoverImage.Should().NotBeNull();
        response.CoverImage!.IsCover.Should().BeTrue();
    }

    #endregion

    #region GeoPointResponse.Map

    [Fact]
    public void GeoPointResponse_Map_MapsAllProperties()
    {
        var geoPoint = new GeoPoint(38.0389m, -1.4917m);

        var response = GeoPointResponse.Map(geoPoint);

        response.Latitude.Should().Be(38.0389m);
        response.Longitude.Should().Be(-1.4917m);
    }

    #endregion

    #region AddressResponse.Map

    [Fact]
    public void AddressResponse_Map_MapsAllProperties()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", location);

        var response = AddressResponse.Map(address);

        response.Street.Should().Be("Ctra. Murcia, 23");
        response.City.Should().Be("La Puebla de Mula");
        response.PostalCode.Should().Be("30193");
        response.Region.Should().Be("Murcia");
        response.Country.Should().Be("España");
        response.Location.Latitude.Should().Be(38.0389m);
        response.Location.Longitude.Should().Be(-1.4917m);
        response.FullAddress.Should().Be("Ctra. Murcia, 23, 30193 La Puebla de Mula, Murcia, España");
    }

    #endregion

    #region ContactInfoResponse.Map

    [Fact]
    public void ContactInfoResponse_Map_MapsAllProperties()
    {
        var contactInfo = new ContactInfo("639079481", "juanjo@example.com", "https://facebook.com/elbardeljuanjo");

        var response = ContactInfoResponse.Map(contactInfo);

        response.Phone.Should().Be("639079481");
        response.Email.Should().Be("juanjo@example.com");
        response.WebsiteUrl.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    [Fact]
    public void ContactInfoResponse_Map_WithNullOptionalFields_MapsNullValues()
    {
        var contactInfo = new ContactInfo("639079481", null, null);

        var response = ContactInfoResponse.Map(contactInfo);

        response.Phone.Should().Be("639079481");
        response.Email.Should().BeNull();
        response.WebsiteUrl.Should().BeNull();
    }

    #endregion

    #region BillingInfoResponse.Map

    [Fact]
    public void BillingInfoResponse_Map_MapsAllProperties()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var billingAddress = new Address("C/ Gran Vía, 1", "Murcia", "30001", "Murcia", "España", location);
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", billingAddress);

        var response = BillingInfoResponse.Map(billingInfo);

        response.BusinessName.Should().Be("Bar Juanjo SL");
        response.TaxId.Should().Be("B12345678");
        response.BillingAddress.Street.Should().Be("C/ Gran Vía, 1");
        response.BillingAddress.City.Should().Be("Murcia");
    }

    #endregion

    #region PriceRangeResponse.Map

    [Fact]
    public void PriceRangeResponse_Map_MapsAllProperties()
    {
        var priceRange = new PriceRange(10m, 30m);

        var response = PriceRangeResponse.Map(priceRange);

        response.MinPrice.Should().Be(10m);
        response.MaxPrice.Should().Be(30m);
    }

    #endregion

    #region CultureCodeResponse.Map

    [Fact]
    public void CultureCodeResponse_Map_MapsAllProperties()
    {
        var cultureCode = new CultureCode("es-ES");

        var response = CultureCodeResponse.Map(cultureCode);

        response.Code.Should().Be("es-ES");
    }

    #endregion

    #region SocialLinkResponse.Map

    [Fact]
    public void SocialLinkResponse_Map_MapsAllProperties()
    {
        var socialLink = new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo");

        var response = SocialLinkResponse.Map(socialLink);

        response.Platform.Should().Be("Facebook");
        response.Url.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    #endregion

    #region CustomerImageResponse.Map

    [Fact]
    public void CustomerImageResponse_Map_MapsAllProperties()
    {
        var imageId = Guid.NewGuid();
        var image = new CustomerImage(imageId, "https://cdn.fudie.com/images/fachada.jpg", "Fachada del bar", 1, true);

        var response = CustomerImageResponse.Map(image);

        response.Id.Should().Be(imageId);
        response.Url.Should().Be("https://cdn.fudie.com/images/fachada.jpg");
        response.AltText.Should().Be("Fachada del bar");
        response.DisplayOrder.Should().Be(1);
        response.IsCover.Should().BeTrue();
    }

    [Fact]
    public void CustomerImageResponse_Map_WithNullAltText_MapsNull()
    {
        var image = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/interior.jpg", null, 0, false);

        var response = CustomerImageResponse.Map(image);

        response.AltText.Should().BeNull();
        response.IsCover.Should().BeFalse();
    }

    #endregion
}