namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Command data for adding a menu item to a category.
/// </summary>
/// <param name="MenuItem">The menu item to add. Must not be null.</param>
/// <param name="DisplayOrder">The display order within the category. Defaults to 0.</param>
/// <param name="PriceOverrides">Optional price overrides for this category context.</param>
public record AddItemCommand(
    MenuItem MenuItem,
    int DisplayOrder = 0,
    HashSet<PriceOption>? PriceOverrides = null
);

public static class AddItemValidationMessages
{
    public const string ItemAlreadyExists = "This item already exists in the category";
}

public partial class MenuCategory
{
    /// <summary>
    /// Command handler for adding a <see cref="CategoryItem"/> to a category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a menu item to the category's collection. Each menu item can only
    /// appear once per category (enforced by CategoryItem equality on MenuItem.Id).
    /// </para>
    /// <para>
    /// The command uses <see cref="CategoryItem.Create"/> to instantiate the category item
    /// and validates the category after modification.
    /// </para>
    /// </remarks>
    /// <param name="createCategoryItem">The command handler for creating category items.</param>
    /// <param name="categoryValidator">The validator for category instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddItem(
        CategoryItem.Create createCategoryItem,
        IValidator<MenuCategory> categoryValidator
    ) : AbstractModifyCommand<AddItemCommand, MenuCategory>
    {
        /// <summary>
        /// Executes the add item command.
        /// </summary>
        /// <param name="category">The category to add the item to.</param>
        /// <param name="command">The command containing the item data.</param>
        /// <returns>The updated and validated <see cref="MenuCategory"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the item already exists in the category.</exception>
        /// <exception cref="ValidationException">Thrown when the item or category data is invalid.</exception>
        public override MenuCategory Execute(MenuCategory category, AddItemCommand command)
        {
            var itemExists = category._items.Any(i => i.MenuItem.Id == command.MenuItem.Id);

            ConflictGuard.ThrowIf(
                itemExists,
                AddItemValidationMessages.ItemAlreadyExists
            );

            var item = createCategoryItem.Execute(new CreateCategoryItemCommand(
                command.MenuItem,
                command.DisplayOrder,
                command.PriceOverrides
            ));

            category._items.Add(item);

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
