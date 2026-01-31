namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Command data for updating a menu item within a category.
/// </summary>
/// <param name="MenuItemId">The unique identifier of the menu item to update.</param>
/// <param name="DisplayOrder">The new display order within the category.</param>
/// <param name="PriceOverrides">Optional new price overrides for this category context.</param>
public record UpdateItemCommand(
    Guid MenuItemId,
    int DisplayOrder,
    HashSet<PriceOption>? PriceOverrides
);

public static class UpdateItemValidationMessages
{
    public const string ItemNotFoundInCategory = "Item not found in category";
}

public partial class MenuCategory
{
    /// <summary>
    /// Command handler for updating a <see cref="CategoryItem"/> within a category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the display order and price overrides for an existing item
    /// in the category. It replaces the existing CategoryItem with a new one containing
    /// the updated values.
    /// </para>
    /// <para>
    /// The command validates that the item exists in the category before updating.
    /// </para>
    /// </remarks>
    /// <param name="createCategoryItem">The command handler for creating category items.</param>
    /// <param name="categoryValidator">The validator for category instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateItem(
        CategoryItem.Create createCategoryItem,
        IValidator<MenuCategory> categoryValidator
    ) : AbstractModifyCommand<UpdateItemCommand, MenuCategory>
    {
        /// <summary>
        /// Executes the update item command.
        /// </summary>
        /// <param name="category">The category containing the item to update.</param>
        /// <param name="command">The command containing the updated item data.</param>
        /// <returns>The updated and validated <see cref="MenuCategory"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the item is not found in the category.</exception>
        /// <exception cref="ValidationException">Thrown when the item data is invalid.</exception>
        public override MenuCategory Execute(MenuCategory category, UpdateItemCommand command)
        {
            var existingItem = category._items.FirstOrDefault(i => i.MenuItem.Id == command.MenuItemId);

            NotFoundGuard.ThrowIfNull(existingItem, UpdateItemValidationMessages.ItemNotFoundInCategory);

            var updatedItem = createCategoryItem.Execute(new CreateCategoryItemCommand(
                existingItem!.MenuItem,
                command.DisplayOrder,
                command.PriceOverrides
            ));

            category._items.Remove(existingItem);
            category._items.Add(updatedItem);

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
