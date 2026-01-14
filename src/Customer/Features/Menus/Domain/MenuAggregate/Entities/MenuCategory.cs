namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Represents a category within a menu that groups related menu items.
/// </summary>
/// <remarks>
/// <para>
/// This entity follows the MicroDomain pattern where business logic is organized
/// in separate command classes (partial class). Categories belong to the <see cref="Menu"/>
/// aggregate and contain a collection of <see cref="MenuItem"/> instances.
/// </para>
/// <para>
/// Examples of categories include "Appetizers", "Main Courses", "Desserts", "Beverages", etc.
/// </para>
/// </remarks>
public partial class MenuCategory : Entity
{
    /// <summary>
    /// Gets the name of the category.
    /// </summary>
    /// <value>The category name. Maximum length is 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the optional description of the category.
    /// </summary>
    /// <value>The category description, or <c>null</c> if not specified. Maximum length is 500 characters.</value>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets the display order for sorting categories within a menu.
    /// </summary>
    /// <value>A non-negative integer representing the display order.</value>
    public int DisplayOrder { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the category is currently active.
    /// </summary>
    /// <value><c>true</c> if the category is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// The internal collection of menu items in this category.
    /// </summary>
    protected HashSet<MenuItem> _items = [];

    /// <summary>
    /// Gets the read-only collection of items in this category.
    /// </summary>
    /// <value>A read-only collection of <see cref="MenuItem"/> instances.</value>
    public IReadOnlyCollection<MenuItem> Items => _items.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCategory"/> class for ORM purposes.
    /// </summary>
    protected MenuCategory() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCategory"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the category.</param>
    public MenuCategory(Guid id) : base(id) { }
}

/// <summary>
/// Provides validation rules for the <see cref="MenuCategory"/> entity.
/// </summary>
/// <remarks>
/// This validator ensures that category properties comply with business rules,
/// including required fields and length constraints.
/// </remarks>
public class MenuCategoryValidator : AbstractValidator<MenuCategory>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCategoryValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public MenuCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Category Id is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be greater than or equal to 0");
    }
}
