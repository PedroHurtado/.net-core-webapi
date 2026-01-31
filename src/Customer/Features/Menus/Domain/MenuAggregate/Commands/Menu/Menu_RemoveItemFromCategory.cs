namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for removing an item from a category.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category.</param>
/// <param name="MenuItemId">The unique identifier of the menu item to remove.</param>
public record RemoveItemFromCategoryCommand(
    Guid CategoryId,
    Guid MenuItemId
);

public static class RemoveItemFromCategoryValidationMessages
{
    public const string CategoryNotFound = "Category not found";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for removing a <see cref="CategoryItem"/> from a <see cref="MenuCategory"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes a menu item from a specific category within the menu.
    /// It delegates to <see cref="MenuCategory.RemoveItem"/> for the actual item removal.
    /// The MenuItem itself is not deleted, only the association with the category is removed.
    /// </para>
    /// <para>
    /// The command validates that the category exists before delegating the removal.
    /// </para>
    /// </remarks>
    /// <param name="removeItem">The command handler for removing items from categories.</param>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveItemFromCategory(
        MenuCategory.RemoveItem removeItem,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<RemoveItemFromCategoryCommand, Menu>
    {
        /// <summary>
        /// Executes the remove item from category command.
        /// </summary>
        /// <param name="menu">The menu containing the category and item.</param>
        /// <param name="command">The command containing the category and item identifiers.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the category or item is not found.</exception>
        public override Menu Execute(Menu menu, RemoveItemFromCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, RemoveItemFromCategoryValidationMessages.CategoryNotFound);

            removeItem.Execute(category!, new RemoveItemCommand(command.MenuItemId));

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
