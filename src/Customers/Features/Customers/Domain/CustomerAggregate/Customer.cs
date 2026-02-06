namespace Customers.Features.Customers.Domain.CustomerAggregate;

/// <summary>
/// Represents a customer aggregate root, the central entity for managing
/// establishment profiles, contact details, billing, images, and social presence.
/// </summary>
/// <remarks>
/// <para>The aggregate uses a partial class pattern to keep commands in separate files.</para>
/// <para>A customer starts inactive and must complete its profile before activation.</para>
/// </remarks>
public partial class Customer : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the display name of the customer.
    /// </summary>
    /// <value>A non-empty string up to 150 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the URL-friendly identifier of the customer.
    /// </summary>
    /// <value>A non-empty string of lowercase letters, numbers, and hyphens up to 150 characters.</value>
    public string Slug { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the optional description of the customer.
    /// </summary>
    /// <value>An optional string up to 2000 characters.</value>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets the optional logo URL of the customer.
    /// </summary>
    /// <value>An optional valid URL up to 500 characters.</value>
    public string? LogoUrl { get; protected set; }

    /// <summary>
    /// Gets the type of establishment.
    /// </summary>
    /// <value>A non-empty string up to 100 characters (e.g. "Bar", "Restaurante").</value>
    public string EstablishmentType { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the default culture code for the customer.
    /// </summary>
    /// <value>A culture code in format 'xx-XX' (e.g. "es-ES").</value>
    public string DefaultCulture { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the IANA time zone identifier.
    /// </summary>
    /// <value>A non-empty string up to 100 characters (e.g. "Europe/Madrid").</value>
    public string TimeZoneId { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the customer is active.
    /// </summary>
    /// <value><c>true</c> if the customer is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets the physical address of the customer.
    /// </summary>
    /// <value>A non-null <see cref="ValueObjects.Address"/> instance.</value>
    public Address Address { get; protected set; } = default!;

    /// <summary>
    /// Gets the contact information of the customer.
    /// </summary>
    /// <value>A non-null <see cref="ValueObjects.ContactInfo"/> instance.</value>
    public ContactInfo ContactInfo { get; protected set; } = default!;

    /// <summary>
    /// Gets the billing information of the customer.
    /// </summary>
    /// <value>A non-null <see cref="ValueObjects.BillingInfo"/> instance.</value>
    public BillingInfo BillingInfo { get; protected set; } = default!;

    /// <summary>
    /// Gets the optional price range of the customer.
    /// </summary>
    /// <value>A nullable <see cref="ValueObjects.PriceRange"/> instance.</value>
    public PriceRange? PriceRange { get; protected set; }

    /// <summary>
    /// The internal collection of images.
    /// </summary>
    protected HashSet<CustomerImage> _images = [];

    /// <summary>
    /// Gets the read-only collection of images.
    /// </summary>
    /// <value>A read-only collection of <see cref="CustomerImage"/> instances.</value>
    public IReadOnlyCollection<CustomerImage> Images => _images.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of cuisine types.
    /// </summary>
    protected HashSet<string> _cuisineTypes = [];

    /// <summary>
    /// Gets the read-only collection of cuisine types.
    /// </summary>
    /// <value>A read-only collection of cuisine type strings.</value>
    public IReadOnlyCollection<string> CuisineTypes => _cuisineTypes.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of service amenities.
    /// </summary>
    protected HashSet<string> _serviceAmenities = [];

    /// <summary>
    /// Gets the read-only collection of service amenities.
    /// </summary>
    /// <value>A read-only collection of service amenity strings.</value>
    public IReadOnlyCollection<string> ServiceAmenities => _serviceAmenities.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of dietary options.
    /// </summary>
    protected HashSet<string> _dietaryOptions = [];

    /// <summary>
    /// Gets the read-only collection of dietary options.
    /// </summary>
    /// <value>A read-only collection of dietary option strings.</value>
    public IReadOnlyCollection<string> DietaryOptions => _dietaryOptions.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of supported cultures.
    /// </summary>
    protected HashSet<CultureCode> _supportedCultures = [];

    /// <summary>
    /// Gets the read-only collection of supported cultures.
    /// </summary>
    /// <value>A read-only collection of <see cref="CultureCode"/> instances.</value>
    public IReadOnlyCollection<CultureCode> SupportedCultures => _supportedCultures.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of social links.
    /// </summary>
    protected HashSet<SocialLink> _socialLinks = [];

    /// <summary>
    /// Gets the read-only collection of social links.
    /// </summary>
    /// <value>A read-only collection of <see cref="SocialLink"/> instances.</value>
    public IReadOnlyCollection<SocialLink> SocialLinks => _socialLinks.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the customer has a price range configured.
    /// </summary>
    /// <value><c>true</c> if the price range is set; otherwise, <c>false</c>.</value>
    public bool HasPriceRange => PriceRange != null;

    /// <summary>
    /// Gets a value indicating whether the customer has a logo.
    /// </summary>
    /// <value><c>true</c> if the logo URL is set; otherwise, <c>false</c>.</value>
    public bool HasLogo => !string.IsNullOrEmpty(LogoUrl);

    /// <summary>
    /// Gets a value indicating whether the customer has any images.
    /// </summary>
    /// <value><c>true</c> if there is at least one image; otherwise, <c>false</c>.</value>
    public bool HasImages => _images.Any();

    /// <summary>
    /// Gets the cover image, or the first image by display order if no cover is set.
    /// </summary>
    /// <value>The cover <see cref="CustomerImage"/>, or <c>null</c> if no images exist.</value>
    public CustomerImage? CoverImage => _images.FirstOrDefault(i => i.IsCover) ?? _images.OrderBy(i => i.DisplayOrder).FirstOrDefault();

    /// <summary>
    /// Gets a value indicating whether the customer profile is complete.
    /// </summary>
    /// <value><c>true</c> if the customer has a logo, images, description, and cuisine types; otherwise, <c>false</c>.</value>
    public bool IsProfileComplete => HasLogo && HasImages && Description != null && CuisineTypes.Any();

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected Customer() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public Customer(Guid id) : base(id) { }
}

public static class CustomerValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 150 characters";
    public const string SlugRequired = "Slug is required";
    public const string SlugMaxLength = "Slug cannot exceed 150 characters";
    public const string SlugFormat = "Slug must contain only lowercase letters, numbers, and hyphens";
    public const string DescriptionMaxLength = "Description cannot exceed 2000 characters";
    public const string LogoUrlMaxLength = "Logo URL cannot exceed 500 characters";
    public const string LogoUrlFormat = "Logo URL must be a valid URL";
    public const string EstablishmentTypeRequired = "Establishment type is required";
    public const string EstablishmentTypeMaxLength = "Establishment type cannot exceed 100 characters";
    public const string DefaultCultureRequired = "Default culture is required";
    public const string DefaultCultureFormat = "Default culture must follow format 'xx-XX' (e.g. es-ES)";
    public const string TimeZoneRequired = "Time zone is required";
    public const string TimeZoneMaxLength = "Time zone cannot exceed 100 characters";
    public const string AddressRequired = "Address is required";
    public const string ContactInfoRequired = "Contact info is required";
    public const string BillingInfoRequired = "Billing info is required";
}

/// <summary>
/// Provides validation rules for the <see cref="Customer"/> aggregate root.
/// </summary>
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.IdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.NameRequired)
            .MaximumLength(150)
            .WithMessage(CustomerValidationMessages.NameMaxLength);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.SlugRequired)
            .MaximumLength(150)
            .WithMessage(CustomerValidationMessages.SlugMaxLength)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage(CustomerValidationMessages.SlugFormat);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage(CustomerValidationMessages.DescriptionMaxLength)
            .When(x => x.Description != null);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .WithMessage(CustomerValidationMessages.LogoUrlMaxLength)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage(CustomerValidationMessages.LogoUrlFormat)
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.EstablishmentType)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.EstablishmentTypeRequired)
            .MaximumLength(100)
            .WithMessage(CustomerValidationMessages.EstablishmentTypeMaxLength);

        RuleFor(x => x.DefaultCulture)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.DefaultCultureRequired)
            .Matches(@"^[a-z]{2}-[A-Z]{2}$")
            .WithMessage(CustomerValidationMessages.DefaultCultureFormat);

        RuleFor(x => x.TimeZoneId)
            .NotEmpty()
            .WithMessage(CustomerValidationMessages.TimeZoneRequired)
            .MaximumLength(100)
            .WithMessage(CustomerValidationMessages.TimeZoneMaxLength);

        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage(CustomerValidationMessages.AddressRequired);

        RuleFor(x => x.ContactInfo)
            .NotNull()
            .WithMessage(CustomerValidationMessages.ContactInfoRequired);

        RuleFor(x => x.BillingInfo)
            .NotNull()
            .WithMessage(CustomerValidationMessages.BillingInfoRequired);
    }
}