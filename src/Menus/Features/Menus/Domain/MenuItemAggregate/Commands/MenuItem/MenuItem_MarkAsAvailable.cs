namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for marking a menu item as available.
/// </summary>
/// <remarks>
/// This command sets the menu item's IsAvailable property to true.
/// This is an idempotent operation - calling it on an already available item has no effect.
/// </remarks>
public record MarkMenuItemAsAvailableCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for marking an existing <see cref="MenuItem"/> as available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the menu item's IsAvailable property to true.
    /// </para>
    /// <para>
    /// This is an idempotent operation - calling it multiple times has the same effect as calling it once.
    /// No conflict exception is thrown if the item is already available.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class MarkAsAvailable(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<MarkMenuItemAsAvailableCommand, MenuItem>
    {
        /// <summary>
        /// Executes the mark as available command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to mark as available.</param>
        /// <param name="command">The mark as available command (empty).</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        public override MenuItem Execute(MenuItem menuItem, MarkMenuItemAsAvailableCommand command)
        {
            menuItem.IsAvailable = true;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
