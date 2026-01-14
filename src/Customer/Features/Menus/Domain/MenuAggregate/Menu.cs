namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Represents a restaurant menu as an aggregate root in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// This class follows the MicroDomain pattern where business logic is organized
/// in separate command classes (partial class). The Menu aggregate manages categories
/// and their items, along with deposit policies for reservations.
/// </para>
/// <para>
/// The menu can have an optional validity period defined by <see cref="EffectiveFrom"/>
/// and <see cref="EffectiveUntil"/> properties.
/// </para>
/// </remarks>
public partial class Menu : AggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the restaurant that owns this menu.
    /// </summary>
    public Guid RestaurantId { get; protected set; }

    /// <summary>
    /// Gets the name of the menu.
    /// </summary>
    /// <value>The menu name. Maximum length is 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the optional description of the menu.
    /// </summary>
    /// <value>The menu description, or <c>null</c> if not specified. Maximum length is 500 characters.</value>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the menu is currently active.
    /// </summary>
    /// <value><c>true</c> if the menu is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets the display order for sorting menus in the UI.
    /// </summary>
    /// <value>A non-negative integer representing the display order.</value>
    public int DisplayOrder { get; protected set; }

    /// <summary>
    /// Gets the date and time from which this menu becomes effective.
    /// </summary>
    /// <value>The effective start date, or <c>null</c> if the menu has no start date restriction.</value>
    public DateTime? EffectiveFrom { get; protected set; }

    /// <summary>
    /// Gets the date and time until which this menu remains effective.
    /// </summary>
    /// <value>The effective end date, or <c>null</c> if the menu has no end date restriction.</value>
    public DateTime? EffectiveUntil { get; protected set; }

    /// <summary>
    /// Gets the deposit policy applied to reservations using this menu.
    /// </summary>
    /// <value>The deposit policy, or <c>null</c> if no deposit is required.</value>
    public DepositPolicy? DepositPolicy { get; protected set; }

    /// <summary>
    /// The internal collection of menu categories.
    /// </summary>
    protected HashSet<MenuCategory> _categories = [];

    /// <summary>
    /// Gets the read-only collection of categories in this menu.
    /// </summary>
    /// <value>A read-only collection of <see cref="MenuCategory"/> instances.</value>
    public IReadOnlyCollection<MenuCategory> Categories => _categories.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Menu"/> class for ORM purposes.
    /// </summary>
    protected Menu() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Menu"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the menu.</param>
    public Menu(Guid id) : base(id) { }
}

public static class MenuValidationMessages
{
    public const string Required = "{PropertyName} is required";
    public const string MaxLength = "{PropertyName} cannot exceed {MaxLength} characters";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
    public const string StartDateMustBeEarlierThanEndDate = "Start date must be earlier than end date";
}

/// <summary>
/// Provides validation rules for the <see cref="Menu"/> aggregate root.
/// </summary>
/// <remarks>
/// This validator ensures that menu properties comply with business rules,
/// including required fields, length constraints, and date range validation.
/// Note that <see cref="DepositPolicy"/> is validated in its own factory method.
/// </remarks>
public class MenuValidator : AbstractValidator<Menu>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public MenuValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(MenuValidationMessages.Required);

        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage(MenuValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MenuValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(MenuValidationMessages.MaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(MenuValidationMessages.MaxLength);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MenuValidationMessages.MinValue);

        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveUntil.HasValue
                       || x.EffectiveFrom < x.EffectiveUntil)
            .WithMessage(MenuValidationMessages.StartDateMustBeEarlierThanEndDate)
            .WithName("EffectiveFrom");
    }
}
