namespace Menus.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Command data for removing a menu item from a category.
/// </summary>
/// <param name="MenuItemId">The unique identifier of the menu item to remove.</param>
public record RemoveItemCommand(Guid MenuItemId);

public static class RemoveItemValidationMessages
{
    public const string ItemNotFoundInCategory = "Item not found in category";
}

public partial class MenuCategory
{
    /// <summary>
    /// Command handler for removing a <see cref="CategoryItem"/> from a category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes a menu item from the category. The MenuItem itself is not
    /// deleted, only the association with the category is removed.
    /// </para>
    /// <para>
    /// The command validates that the item exists in the category before removing.
    /// </para>
    /// </remarks>
    /// <param name="categoryValidator">The validator for category instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveItem(
        IValidator<MenuCategory> categoryValidator
    ) : AbstractModifyCommand<RemoveItemCommand, MenuCategory>
    {
        /// <summary>
        /// Executes the remove item command.
        /// </summary>
        /// <param name="category">The category to remove the item from.</param>
        /// <param name="command">The command containing the item identifier.</param>
        /// <returns>The updated and validated <see cref="MenuCategory"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the item is not found in the category.</exception>
        public override MenuCategory Execute(MenuCategory category, RemoveItemCommand command)
        {
            var item = category._items.FirstOrDefault(i => i.MenuItem.Id == command.MenuItemId);

            NotFoundGuard.ThrowIfNull(item, RemoveItemValidationMessages.ItemNotFoundInCategory);

            category._items.Remove(item!);

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
