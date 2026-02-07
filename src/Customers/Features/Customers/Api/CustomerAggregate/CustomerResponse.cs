namespace Customers.Features.Customers.Api.CustomerAggregate;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    bool IsActive,
    bool HasPriceRange,
    bool HasLogo,
    bool HasImages,
    bool IsProfileComplete,
    AddressResponse Address,
    ContactInfoResponse ContactInfo,
    BillingInfoResponse BillingInfo,
    PriceRangeResponse? PriceRange,
    CustomerImageResponse? CoverImage,
    IReadOnlyCollection<CustomerImageResponse> Images,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> ServiceAmenities,
    IReadOnlyCollection<string> DietaryOptions,
    IReadOnlyCollection<CultureCodeResponse> SupportedCultures,
    IReadOnlyCollection<SocialLinkResponse> SocialLinks)
{
    public static CustomerResponse Map(Customer customer) => new(
        Id: customer.Id,
        Name: customer.Name,
        Slug: customer.Slug,
        Description: customer.Description,
        LogoUrl: customer.LogoUrl,
        EstablishmentType: customer.EstablishmentType,
        DefaultCulture: customer.DefaultCulture,
        TimeZoneId: customer.TimeZoneId,
        IsActive: customer.IsActive,
        HasPriceRange: customer.HasPriceRange,
        HasLogo: customer.HasLogo,
        HasImages: customer.HasImages,
        IsProfileComplete: customer.IsProfileComplete,
        Address: AddressResponse.Map(customer.Address),
        ContactInfo: ContactInfoResponse.Map(customer.ContactInfo),
        BillingInfo: BillingInfoResponse.Map(customer.BillingInfo),
        PriceRange: customer.PriceRange is not null
            ? PriceRangeResponse.Map(customer.PriceRange)
            : null,
        CoverImage: customer.CoverImage is not null
            ? CustomerImageResponse.Map(customer.CoverImage)
            : null,
        Images: customer.Images
            .Select(CustomerImageResponse.Map)
            .ToList()
            .AsReadOnly(),
        CuisineTypes: customer.CuisineTypes,
        ServiceAmenities: customer.ServiceAmenities,
        DietaryOptions: customer.DietaryOptions,
        SupportedCultures: customer.SupportedCultures
            .Select(CultureCodeResponse.Map)
            .ToList()
            .AsReadOnly(),
        SocialLinks: customer.SocialLinks
            .Select(SocialLinkResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record GeoPointResponse(
    decimal Latitude,
    decimal Longitude)
{
    public static GeoPointResponse Map(GeoPoint geoPoint) => new(
        Latitude: geoPoint.Latitude,
        Longitude: geoPoint.Longitude);
}

public record AddressResponse(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    GeoPointResponse Location,
    string FullAddress)
{
    public static AddressResponse Map(Address address) => new(
        Street: address.Street,
        City: address.City,
        PostalCode: address.PostalCode,
        Region: address.Region,
        Country: address.Country,
        Location: GeoPointResponse.Map(address.Location),
        FullAddress: address.FullAddress);
}

public record ContactInfoResponse(
    string Phone,
    string? Email,
    string? WebsiteUrl)
{
    public static ContactInfoResponse Map(ContactInfo contactInfo) => new(
        Phone: contactInfo.Phone,
        Email: contactInfo.Email,
        WebsiteUrl: contactInfo.WebsiteUrl);
}

public record BillingInfoResponse(
    string BusinessName,
    string TaxId,
    AddressResponse BillingAddress)
{
    public static BillingInfoResponse Map(BillingInfo billingInfo) => new(
        BusinessName: billingInfo.BusinessName,
        TaxId: billingInfo.TaxId,
        BillingAddress: AddressResponse.Map(billingInfo.BillingAddress));
}

public record PriceRangeResponse(
    decimal MinPrice,
    decimal MaxPrice)
{
    public static PriceRangeResponse Map(PriceRange priceRange) => new(
        MinPrice: priceRange.MinPrice,
        MaxPrice: priceRange.MaxPrice);
}

public record CultureCodeResponse(
    string Code)
{
    public static CultureCodeResponse Map(CultureCode cultureCode) => new(
        Code: cultureCode.Code);
}

public record SocialLinkResponse(
    string Platform,
    string Url)
{
    public static SocialLinkResponse Map(SocialLink socialLink) => new(
        Platform: socialLink.Platform,
        Url: socialLink.Url);
}

public record CustomerImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsCover)
{
    public static CustomerImageResponse Map(CustomerImage image) => new(
        Id: image.Id,
        Url: image.Url,
        AltText: image.AltText,
        DisplayOrder: image.DisplayOrder,
        IsCover: image.IsCover);
}