namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents a physical address with geographic coordinates.
/// </summary>
/// <remarks>
/// <para>Contains street, city, postal code, region, country, and a geographic location point.</para>
/// <para>Provides a computed FullAddress property for display purposes.</para>
/// </remarks>
public partial record Address(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    GeoPoint Location
)
{
    public string FullAddress => $"{Street}, {PostalCode} {City}, {Region}, {Country}";
}

public static class AddressValidationMessages
{
    public const string StreetRequired = "Street is required";
    public const string StreetMaxLength = "Street cannot exceed 200 characters";
    public const string CityRequired = "City is required";
    public const string CityMaxLength = "City cannot exceed 100 characters";
    public const string PostalCodeRequired = "Postal code is required";
    public const string PostalCodeMaxLength = "Postal code cannot exceed 20 characters";
    public const string RegionRequired = "Region is required";
    public const string RegionMaxLength = "Region cannot exceed 100 characters";
    public const string CountryRequired = "Country is required";
    public const string CountryMaxLength = "Country cannot exceed 100 characters";
    public const string LocationRequired = "Location is required";
}

/// <summary>
/// Provides validation rules for the <see cref="Address"/> value object.
/// </summary>
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage(AddressValidationMessages.StreetRequired);

        RuleFor(x => x.Street)
            .MaximumLength(200)
            .WithMessage(AddressValidationMessages.StreetMaxLength);

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage(AddressValidationMessages.CityRequired);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage(AddressValidationMessages.CityMaxLength);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage(AddressValidationMessages.PostalCodeRequired);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage(AddressValidationMessages.PostalCodeMaxLength);

        RuleFor(x => x.Region)
            .NotEmpty()
            .WithMessage(AddressValidationMessages.RegionRequired);

        RuleFor(x => x.Region)
            .MaximumLength(100)
            .WithMessage(AddressValidationMessages.RegionMaxLength);

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage(AddressValidationMessages.CountryRequired);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage(AddressValidationMessages.CountryMaxLength);

        RuleFor(x => x.Location)
            .NotNull()
            .WithMessage(AddressValidationMessages.LocationRequired);
    }
}
