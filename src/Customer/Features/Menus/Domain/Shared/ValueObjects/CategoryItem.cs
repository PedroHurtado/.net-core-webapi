namespace Customer.Features.Menus.Domain.Shared.ValueObjects;

/// <summary>
/// Represents a menu item within a category with its display order and optional price overrides.
/// </summary>
/// <remarks>
/// <para>
/// This value object associates a <see cref="MenuItemAggregate.MenuItem"/> with a category,
/// allowing for custom display ordering and price overrides specific to that category context.
/// </para>
/// <para>
/// Two <see cref="CategoryItem"/> instances are considered equal if they reference the same
/// <see cref="MenuItem"/> (equality by MenuItem reference).
/// </para>
/// </remarks>
public record CategoryItem
{
    /// <summary>
    /// Gets the menu item associated with this category item.
    /// </summary>
    /// <remarks>
    /// Stored as a reference in Firestore for efficient querying and data integrity.
    /// </remarks>
    public MenuItem MenuItem { get; }

    /// <summary>
    /// Gets the display order for sorting this item within the category.
    /// </summary>
    /// <value>A non-negative integer representing the display order.</value>
    public int DisplayOrder { get; }

    /// <summary>
    /// The internal collection of price overrides for this item in this category context.
    /// </summary>
    private readonly HashSet<PriceOption> _priceOverrides;

    /// <summary>
    /// Gets the read-only collection of price overrides for this item in this category.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PriceOption"/> instances that override
    /// the menu item's default prices for this specific category context.
    /// </value>
    /// <remarks>
    /// When empty, the menu item's default <see cref="MenuItem.PriceOptions"/> are used.
    /// </remarks>
    public IReadOnlyCollection<PriceOption> PriceOverrides => _priceOverrides.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryItem"/> record.
    /// </summary>
    /// <param name="menuItem">The menu item reference.</param>
    /// <param name="displayOrder">The display order within the category.</param>
    /// <param name="priceOverrides">Optional price overrides for this category context.</param>
    private CategoryItem(
        MenuItem menuItem,
        int displayOrder,
        HashSet<PriceOption>? priceOverrides)
    {
        MenuItem = menuItem;
        DisplayOrder = displayOrder;
        _priceOverrides = priceOverrides ?? [];
    }

    /// <summary>
    /// Creates a new validated <see cref="CategoryItem"/> instance.
    /// </summary>
    /// <param name="menuItem">The menu item reference. Must not be null.</param>
    /// <param name="displayOrder">The display order within the category. Must be non-negative.</param>
    /// <param name="priceOverrides">Optional price overrides for this category context.</param>
    /// <returns>A validated <see cref="CategoryItem"/> instance.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static CategoryItem Create(
        MenuItem menuItem,
        int displayOrder = 0,
        HashSet<PriceOption>? priceOverrides = null)
    {
        var item = new CategoryItem(menuItem, displayOrder, priceOverrides);

        return new CategoryItemValidator().ValidateOrThrow(item);
    }

    /// <summary>
    /// Determines whether two <see cref="CategoryItem"/> instances are equal.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><c>true</c> if both reference the same <see cref="MenuItem"/>; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Equality is based solely on the MenuItem reference, not on DisplayOrder or PriceOverrides.
    /// </remarks>
    public virtual bool Equals(CategoryItem? other)
    {
        if (other is null) return false;
        return MenuItem.Id == other.MenuItem.Id;
    }

    /// <summary>
    /// Gets the hash code based on the MenuItem's Id.
    /// </summary>
    /// <returns>The hash code of the MenuItem's Id.</returns>
    public override int GetHashCode() => MenuItem.Id.GetHashCode();
}

public static class CategoryItemValidationMessages
{
    public const string Required = "{PropertyName} is required";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
}

/// <summary>
/// Provides validation rules for the <see cref="CategoryItem"/> value object.
/// </summary>
/// <remarks>
/// Validates category item invariants including required menu item reference
/// and non-negative display order.
/// </remarks>
public class CategoryItemValidator : AbstractValidator<CategoryItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryItemValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public CategoryItemValidator()
    {
        RuleFor(x => x.MenuItem)
            .NotNull()
            .WithMessage(CategoryItemValidationMessages.Required);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(CategoryItemValidationMessages.MinValue);
    }
}
