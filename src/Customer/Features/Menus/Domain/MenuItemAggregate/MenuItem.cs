namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Represents a menu item as an aggregate root that can be shared across different menu types.
/// </summary>
/// <remarks>
/// <para>
/// This aggregate follows the MicroDomain pattern where business logic is organized
/// in separate command classes (partial class). Menu items can be referenced by
/// multiple aggregates such as <c>Menu</c> and <c>MenuDay</c>.
/// </para>
/// <para>
/// Items can have multiple price options for different portion sizes, nutritional information,
/// allergen data, and special ordering requirements (such as advance orders for high-risk items).
/// </para>
/// </remarks>
public partial class MenuItem : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the unique identifier of the restaurant that owns this menu item.
    /// </summary>
    public Guid RestaurantId { get; protected set; }

    /// <summary>
    /// Gets the name of the menu item.
    /// </summary>
    /// <value>The item name. Maximum length is 150 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the optional description of the menu item.
    /// </summary>
    /// <value>The item description, or <c>null</c> if not specified. Maximum length is 1000 characters.</value>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets the URL of the item's image.
    /// </summary>
    /// <value>The image URL, or <c>null</c> if no image is available. Maximum length is 500 characters.</value>
    public string? ImageUrl { get; protected set; }

    /// <summary>
    /// Gets the display order for sorting items within a category.
    /// </summary>
    /// <value>A non-negative integer representing the display order.</value>
    public int DisplayOrder { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the item is currently active.
    /// </summary>
    /// <value><c>true</c> if the item is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this is a high-risk item that may require special handling.
    /// </summary>
    /// <value><c>true</c> if the item is high-risk; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// High-risk items typically include expensive ingredients or items that require special preparation.
    /// If <see cref="RequiresAdvanceOrder"/> is <c>true</c>, this property must also be <c>true</c>.
    /// </remarks>
    public bool IsHighRiskItem { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the item requires an advance order.
    /// </summary>
    /// <value><c>true</c> if advance ordering is required; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// When <c>true</c>, <see cref="IsHighRiskItem"/> must also be <c>true</c>.
    /// </remarks>
    public bool RequiresAdvanceOrder { get; protected set; }

    /// <summary>
    /// Gets the minimum quantity that triggers the advance order requirement.
    /// </summary>
    /// <value>The minimum quantity (1-100), or <c>null</c> if not applicable.</value>
    /// <remarks>
    /// When this value is set, <see cref="RequiresAdvanceOrder"/> must be <c>true</c>.
    /// </remarks>
    public int? MinimumAdvanceOrderQuantity { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the item is currently available for ordering.
    /// </summary>
    /// <value><c>true</c> if the item is available; otherwise, <c>false</c>.</value>
    public bool IsAvailable { get; protected set; } = true;

    /// <summary>
    /// Gets a value indicating whether the item is available every day.
    /// </summary>
    /// <value><c>true</c> if always available; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// When <c>false</c>, <see cref="AvailableDays"/> must contain at least one day.
    /// </remarks>
    public bool IsAlwaysAvailable { get; protected set; } = true;

    /// <summary>
    /// Gets the deposit override configuration for this specific item.
    /// </summary>
    /// <value>The deposit override, or <c>null</c> if the menu's default policy applies.</value>
    public ItemDepositOverride? DepositOverride { get; protected set; }

    /// <summary>
    /// Gets the nutritional information for this item.
    /// </summary>
    /// <value>The nutritional info, or <c>null</c> if not available.</value>
    public NutritionalInfo? NutritionalInfo { get; protected set; }

    /// <summary>
    /// The internal collection of days when this item is available.
    /// </summary>
    protected HashSet<DayOfWeek> _availableDays = [];

    /// <summary>
    /// Gets the read-only collection of days when this item is available.
    /// </summary>
    /// <value>A read-only collection of <see cref="DayOfWeek"/> values.</value>
    /// <remarks>
    /// This is only relevant when <see cref="IsAlwaysAvailable"/> is <c>false</c>.
    /// </remarks>
    public IReadOnlyCollection<DayOfWeek> AvailableDays => _availableDays.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of price options for this item.
    /// </summary>
    protected HashSet<PriceOption> _priceOptions = [];

    /// <summary>
    /// Gets the read-only collection of price options for different portion sizes.
    /// </summary>
    /// <value>A read-only collection of <see cref="PriceOption"/> instances. Must contain at least one option.</value>
    public IReadOnlyCollection<PriceOption> PriceOptions => _priceOptions.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of allergens associated with this item.
    /// </summary>
    protected HashSet<Allergen> _allergens = [];

    /// <summary>
    /// Gets the read-only collection of allergens associated with this item.
    /// </summary>
    /// <value>A read-only collection of <see cref="Allergen"/> entities.</value>
    public IReadOnlyCollection<Allergen> Allergens => _allergens.ToList().AsReadOnly();

    /// <summary>
    /// Gets additional notes about allergens for this item.
    /// </summary>
    /// <value>Free-text allergen notes, or <c>null</c> if not specified. Maximum length is 500 characters.</value>
    public string? AllergenNotes { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class for ORM purposes.
    /// </summary>
    protected MenuItem() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the menu item.</param>
    public MenuItem(Guid id) : base(id) { }
}

public static class MenuItemValidationMessages
{
    public const string Required = "{PropertyName} is required";
    public const string MaxLength = "{PropertyName} cannot exceed {MaxLength} characters";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
    public const string InvalidUrl = "{PropertyName} is not valid";
    public const string RequiresAdvanceOrderMustBeHighRisk = "If RequiresAdvanceOrder is true, IsHighRiskItem must also be true";
    public const string MinimumQuantityRange = "{PropertyName} must be between {From} and {To}";
    public const string MinimumAdvanceOrderQuantityRequiresAdvanceOrder = "If MinimumAdvanceOrderQuantity has a value, RequiresAdvanceOrder must be true";
    public const string AvailableDaysRequired = "Available days must be specified if the item is not always available";
    public const string PriceOptionsRequired = "Item must have at least one price option";
}

/// <summary>
/// Provides validation rules for the <see cref="MenuItem"/> aggregate root.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures that menu item properties comply with business rules,
/// including required fields, length constraints, and logical consistency.
/// </para>
/// <para>
/// Note that value objects (<see cref="PriceOption"/>, <see cref="ItemDepositOverride"/>,
/// <see cref="NutritionalInfo"/>) are validated in their own factory methods.
/// </para>
/// </remarks>
public class MenuItemValidator : AbstractValidator<MenuItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItemValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public MenuItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(MenuItemValidationMessages.Required);

        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage(MenuItemValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MenuItemValidationMessages.Required)
            .MaximumLength(150)
            .WithMessage(MenuItemValidationMessages.MaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(MenuItemValidationMessages.MaxLength);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .WithMessage(MenuItemValidationMessages.MaxLength)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage(MenuItemValidationMessages.InvalidUrl);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MenuItemValidationMessages.MinValue);

        RuleFor(x => x)
            .Must(x => !x.RequiresAdvanceOrder || x.IsHighRiskItem)
            .WithMessage(MenuItemValidationMessages.RequiresAdvanceOrderMustBeHighRisk)
            .WithName("RequiresAdvanceOrder");

        RuleFor(x => x.MinimumAdvanceOrderQuantity)
            .InclusiveBetween(1, 100)
            .When(x => x.MinimumAdvanceOrderQuantity.HasValue)
            .WithMessage(MenuItemValidationMessages.MinimumQuantityRange);

        RuleFor(x => x)
            .Must(x => !x.MinimumAdvanceOrderQuantity.HasValue || x.RequiresAdvanceOrder)
            .WithMessage(MenuItemValidationMessages.MinimumAdvanceOrderQuantityRequiresAdvanceOrder)
            .WithName("MinimumAdvanceOrderQuantity");

        RuleFor(x => x.AvailableDays)
            .NotEmpty()
            .When(x => !x.IsAlwaysAvailable)
            .WithMessage(MenuItemValidationMessages.AvailableDaysRequired);

        RuleFor(x => x.PriceOptions)
            .NotEmpty()
            .WithMessage(MenuItemValidationMessages.PriceOptionsRequired);

        RuleFor(x => x.AllergenNotes)
            .MaximumLength(500)
            .WithMessage(MenuItemValidationMessages.MaxLength);
    }

    /// <summary>
    /// Validates that a URL string is a valid absolute HTTP or HTTPS URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><c>true</c> if the URL is valid or empty; otherwise, <c>false</c>.</returns>
    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
