namespace Customers.UnitTests.Helpers;

public class TestableCustomer : Customer
{
    public TestableCustomer(Guid id) : base(id) { }

    public TestableCustomer WithName(string value) { Name = value; return this; }
    public TestableCustomer WithSlug(string value) { Slug = value; return this; }
    public TestableCustomer WithDescription(string? value) { Description = value; return this; }
    public TestableCustomer WithLogoUrl(string? value) { LogoUrl = value; return this; }
    public TestableCustomer WithEstablishmentType(string value) { EstablishmentType = value; return this; }
    public TestableCustomer WithDefaultCulture(string value) { DefaultCulture = value; return this; }
    public TestableCustomer WithTimeZoneId(string value) { TimeZoneId = value; return this; }
    public TestableCustomer WithIsActive(bool value) { IsActive = value; return this; }
    public TestableCustomer WithAddress(Address value) { Address = value; return this; }
    public TestableCustomer WithContactInfo(ContactInfo value) { ContactInfo = value; return this; }
    public TestableCustomer WithBillingInfo(BillingInfo value) { BillingInfo = value; return this; }
    public TestableCustomer WithPriceRange(PriceRange? value) { PriceRange = value; return this; }

    public TestableCustomer WithImage(CustomerImage item) { _images.Add(item); return this; }
    public TestableCustomer WithCuisineType(string item) { _cuisineTypes.Add(item); return this; }
    public TestableCustomer WithServiceAmenity(string item) { _serviceAmenities.Add(item); return this; }
    public TestableCustomer WithDietaryOption(string item) { _dietaryOptions.Add(item); return this; }
    public TestableCustomer WithSupportedCulture(CultureCode item) { _supportedCultures.Add(item); return this; }
    public TestableCustomer WithSocialLink(SocialLink item) { _socialLinks.Add(item); return this; }
}