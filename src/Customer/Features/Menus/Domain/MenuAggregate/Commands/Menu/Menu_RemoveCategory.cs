namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for removing a category from a menu.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category to remove.</param>
public record RemoveCategoryCommand(Guid CategoryId);

public partial class Menu
{
    /// <summary>
    /// Command handler for removing a <see cref="MenuCategory"/> from a menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes a category from the menu's collection. Categories can only
    /// be removed if they contain no items. This prevents orphaned menu items.
    /// </para>
    /// <para>
    /// The command validates the menu after modification.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveCategory(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<RemoveCategoryCommand, Menu>
    {
        /// <summary>
        /// Executes the remove category command.
        /// </summary>
        /// <param name="menu">The menu to remove the category from.</param>
        /// <param name="command">The command containing the category identifier.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the category is not found in the menu.</exception>
        /// <exception cref="ValidationException">Thrown when the category contains items and cannot be removed.</exception>
        public override Menu Execute(Menu menu, RemoveCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, command.CategoryId);

            ValidationGuard.ThrowIf(
                category!.Items.Count != 0,
                "Cannot remove a category that contains items",
                "CategoryId"
            );

            menu._categories.Remove(category);
            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
