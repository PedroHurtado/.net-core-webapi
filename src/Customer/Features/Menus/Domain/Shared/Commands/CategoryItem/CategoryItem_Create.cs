namespace Customer.Features.Menus.Domain.Shared.ValueObjects;

/// <summary>
/// Command data for creating a category item.
/// </summary>
/// <param name="MenuItem">The menu item reference. Must not be null.</param>
/// <param name="DisplayOrder">The display order within the category. Must be non-negative.</param>
/// <param name="PriceOverrides">Optional price overrides for this category context.</param>
public record CreateCategoryItemCommand(
    MenuItem MenuItem,
    int DisplayOrder = 0,
    HashSet<PriceOption>? PriceOverrides = null
);

public partial record CategoryItem
{
    /// <summary>
    /// Command handler for creating a new <see cref="CategoryItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a category item that associates a menu item with a category,
    /// allowing for custom display ordering and price overrides.
    /// </para>
    /// <para>
    /// The command validates the category item using <see cref="CategoryItemValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="categoryItemValidator">The validator for category item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<CategoryItem> categoryItemValidator
    ) : AbstractCreateCommand<CreateCategoryItemCommand, CategoryItem>
    {
        /// <summary>
        /// Executes the create category item command.
        /// </summary>
        /// <param name="command">The command containing the category item creation data.</param>
        /// <returns>A new validated <see cref="CategoryItem"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the category item data is invalid.</exception>
        public override CategoryItem Execute(CreateCategoryItemCommand command)
        {
            var item = new CategoryItem(
                command.MenuItem,
                command.DisplayOrder,
                command.PriceOverrides);

            return categoryItemValidator.ValidateOrThrow(item);
        }
    }
}
