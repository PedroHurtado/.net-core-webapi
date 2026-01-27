namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for removing nutritional information from a menu item.
/// </summary>
/// <remarks>
/// This is an idempotent operation - removing nutritional info from an item
/// that doesn't have one will succeed without error.
/// </remarks>
public record RemoveNutritionalInfoCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for removing nutritional information from an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes the nutritional information from a menu item by setting it to null.
    /// </para>
    /// <para>
    /// This is an idempotent operation - calling it on an item that doesn't have nutritional info
    /// will succeed without error.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveNutritionalInfo(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<RemoveNutritionalInfoCommand, MenuItem>
    {
        /// <summary>
        /// Executes the remove nutritional info command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to remove nutritional info from.</param>
        /// <param name="command">The remove nutritional info command.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        public override MenuItem Execute(MenuItem menuItem, RemoveNutritionalInfoCommand command)
        {
            menuItem.NutritionalInfo = null;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
