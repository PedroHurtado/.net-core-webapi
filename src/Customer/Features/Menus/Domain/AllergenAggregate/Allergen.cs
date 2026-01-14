namespace Customer.Features.Menus.Domain.AllergenAggregate;

/// <summary>
/// Represents an allergen as a catalog entity managed by the system.
/// </summary>
/// <remarks>
/// <para>
/// Allergens are master data that are preloaded by the system and shared across all restaurants.
/// They can be referenced by MenuItems to indicate which allergens are present in the dish.
/// </para>
/// <para>
/// The Id is the unique code of the allergen (e.g., "GLUTEN", "LACTEOS") which serves as
/// the natural identifier. This follows the Identity pattern where the code itself is the ID.
/// </para>
/// </remarks>
public partial class Allergen : AggregateRoot<string>
{
    /// <summary>
    /// Gets the display name of the allergen in the base language.
    /// </summary>
    /// <value>The allergen name. Maximum length is 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the URL of the allergen's icon.
    /// </summary>
    /// <value>The icon URL, or <c>null</c> if no icon is available. Maximum length is 500 characters.</value>
    public string? IconUrl { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the allergen is currently active in the system.
    /// </summary>
    /// <value><c>true</c> if the allergen is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets the display order for sorting allergens in UI.
    /// </summary>
    /// <value>A non-negative integer representing the display order.</value>
    public int DisplayOrder { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Allergen"/> class for ORM purposes.
    /// </summary>
    protected Allergen() : base(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Allergen"/> class with the specified code.
    /// </summary>
    /// <param name="code">The unique code that serves as the allergen's identifier.</param>
    protected Allergen(string code) : base(code) { }

    /// <summary>
    /// Creates a new allergen with the specified properties.
    /// </summary>
    /// <param name="code">The unique code (e.g., "GLUTEN"). Must be uppercase without spaces.</param>
    /// <param name="name">The display name in base language.</param>
    /// <param name="displayOrder">The display order for sorting.</param>
    /// <param name="iconUrl">Optional URL for the allergen icon.</param>
    /// <param name="isActive">Whether the allergen is active. Defaults to true.</param>
    /// <returns>A <see cref="Result{Allergen}"/> containing the created allergen or validation errors.</returns>
    public static Result<Allergen> Create(
        string code,
        string name,
        int displayOrder = 0,
        string? iconUrl = null,
        bool isActive = true)
    {
        var allergen = new Allergen(code)
        {
            Name = name,
            IconUrl = iconUrl,
            IsActive = isActive,
            DisplayOrder = displayOrder
        };

        return allergen.ValidateEntity(allergen, new AllergenValidator());
    }

    /// <summary>
    /// Updates the allergen's properties.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="displayOrder">The new display order.</param>
    /// <param name="iconUrl">The new icon URL.</param>
    /// <param name="isActive">Whether the allergen is active.</param>
    /// <returns>A <see cref="Result"/> indicating success or validation errors.</returns>
    public Result Update(string name, int displayOrder, string? iconUrl, bool isActive)
    {
        var tempAllergen = new Allergen(Id)
        {
            Name = name,
            IconUrl = iconUrl,
            IsActive = isActive,
            DisplayOrder = displayOrder
        };

        var validationResult = tempAllergen.ValidateEntity(tempAllergen, new AllergenValidator());

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        Name = name;
        IconUrl = iconUrl;
        IsActive = isActive;
        DisplayOrder = displayOrder;

        return Result.Success();
    }

    /// <summary>
    /// Deactivates the allergen.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Activates the allergen.
    /// </summary>
    public void Activate() => IsActive = true;
}

/// <summary>
/// Validation messages for the <see cref="Allergen"/> entity.
/// </summary>
public static class AllergenValidationMessages
{
    public const string CodeRequired = "Code is required";
    public const string CodeMaxLength = "Code cannot exceed 20 characters";
    public const string CodeFormat = "Code must be uppercase letters, numbers, and underscores only";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 100 characters";
    public const string IconUrlMaxLength = "IconUrl cannot exceed 500 characters";
    public const string IconUrlInvalid = "IconUrl must be a valid URL";
    public const string DisplayOrderMin = "DisplayOrder must be greater than or equal to 0";
}

/// <summary>
/// Provides validation rules for the <see cref="Allergen"/> entity.
/// </summary>
public class AllergenValidator : AbstractValidator<Allergen>
{
    public AllergenValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(AllergenValidationMessages.CodeRequired)
            .MaximumLength(20)
            .WithMessage(AllergenValidationMessages.CodeMaxLength)
            .Matches(@"^[A-Z0-9_]+$")
            .WithMessage(AllergenValidationMessages.CodeFormat);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(AllergenValidationMessages.NameRequired)
            .MaximumLength(100)
            .WithMessage(AllergenValidationMessages.NameMaxLength);

        RuleFor(x => x.IconUrl)
            .MaximumLength(500)
            .WithMessage(AllergenValidationMessages.IconUrlMaxLength)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.IconUrl))
            .WithMessage(AllergenValidationMessages.IconUrlInvalid);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(AllergenValidationMessages.DisplayOrderMin);
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
