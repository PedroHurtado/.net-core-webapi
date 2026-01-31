namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for adding an item to a category in a menu.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category.</param>
/// <param name="MenuItem">The menu item to add.</param>
/// <param name="DisplayOrder">The display order within the category. Defaults to 0.</param>
/// <param name="PriceOverrides">Optional price overrides for this category context.</param>
public record AddItemToCategoryCommand(
    Guid CategoryId,
    MenuItem MenuItem,
    int DisplayOrder = 0,
    HashSet<PriceOption>? PriceOverrides = null
);

public static class AddItemToCategoryValidationMessages
{
    public const string CategoryNotFound = "Category not found";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for adding a <see cref="MenuItem"/> to a <see cref="MenuCategory"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a menu item to a specific category within the menu.
    /// It delegates to <see cref="MenuCategory.AddItem"/> for the actual item addition.
    /// </para>
    /// <para>
    /// The command validates that the category exists and that the item is not already
    /// present in the category before adding it.
    /// </para>
    /// </remarks>
    /// <param name="addItem">The command handler for adding items to categories.</param>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddItemToCategory(
        MenuCategory.AddItem addItem,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<AddItemToCategoryCommand, Menu>
    {
        /// <summary>
        /// Executes the add item to category command.
        /// </summary>
        /// <param name="menu">The menu containing the category.</param>
        /// <param name="command">The command containing the item data.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the category is not found.</exception>
        /// <exception cref="ConflictException">Thrown when the item already exists in the category.</exception>
        /// <exception cref="ValidationException">Thrown when the item data is invalid.</exception>
        public override Menu Execute(Menu menu, AddItemToCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, AddItemToCategoryValidationMessages.CategoryNotFound);

            addItem.Execute(category!, new AddItemCommand(
                command.MenuItem,
                command.DisplayOrder,
                command.PriceOverrides
            ));

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}