namespace Auth.Infrastructure.Customers;

public interface ICustomerApi
{
    [Post("/customer")]
    Task<CreateCustomerApiResponse> CreateAsync([Body] CreateCustomerApiRequest request);

    [Get("/customer")]
    Task<CustomerApiResponse> GetAsync();

    [Put("/customer")]
    Task UpdateAsync([Body] UpdateCustomerApiRequest request);
}

// ──────────────────────────────────────────────
// Create
// ──────────────────────────────────────────────

public record CreateCustomerApiRequest(
    string Name,
    string Slug,
    string? Description,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    CreateAddressApiRequest Address,
    CreateContactInfoApiRequest ContactInfo,
    CreateBillingInfoApiRequest BillingInfo);

public record CreateAddressApiRequest(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude);

public record CreateContactInfoApiRequest(
    string Phone,
    string? Email,
    string? WebsiteUrl);

public record CreateBillingInfoApiRequest(
    string BusinessName,
    string TaxId,
    CreateAddressApiRequest BillingAddress);

public record CreateCustomerApiResponse(Guid Id, string Name);

// ──────────────────────────────────────────────
// Get
// ──────────────────────────────────────────────

public record CustomerApiResponse(
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    bool IsActive);

// ──────────────────────────────────────────────
// Update
// ──────────────────────────────────────────────

public record UpdateCustomerApiRequest(
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    string[] CuisineTypes,
    string[] ServiceAmenities,
    string[] DietaryOptions);
