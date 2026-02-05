namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for marking a menu item as unavailable.
/// </summary>
/// <remarks>
/// This command sets the menu item's IsAvailable property to false.
/// This is an idempotent operation - calling it on an already unavailable item has no effect.
/// </remarks>
public record MarkMenuItemAsUnavailableCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for marking an existing <see cref="MenuItem"/> as unavailable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the menu item's IsAvailable property to false,
    /// indicating that the item is temporarily out of stock or unavailable.
    /// </para>
    /// <para>
    /// This is an idempotent operation - calling it multiple times has the same effect as calling it once.
    /// No conflict exception is thrown if the item is already unavailable.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class MarkAsUnavailable(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<MarkMenuItemAsUnavailableCommand, MenuItem>
    {
        /// <summary>
        /// Executes the mark as unavailable command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to mark as unavailable.</param>
        /// <param name="command">The mark as unavailable command (empty).</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        public override MenuItem Execute(MenuItem menuItem, MarkMenuItemAsUnavailableCommand command)
        {
            menuItem.IsAvailable = false;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
