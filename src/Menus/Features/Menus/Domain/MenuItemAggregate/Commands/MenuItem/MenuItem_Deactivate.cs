namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for deactivating an existing menu item.
/// </summary>
/// <remarks>
/// This command sets the menu item's IsActive property to false.
/// Following CQRS principles, this is a specific command for a single responsibility.
/// </remarks>
public record DeactivateMenuItemCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for deactivating an existing <see cref="MenuItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the menu item's IsActive property to false.
    /// </para>
    /// <para>
    /// If the menu item is already inactive, a ConflictException is thrown to prevent
    /// redundant operations.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<DeactivateMenuItemCommand, MenuItem>
    {
        /// <summary>
        /// Executes the deactivate menu item command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to deactivate.</param>
        /// <param name="command">The deactivate command (empty).</param>
        /// <returns>The deactivated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the menu item is already inactive.</exception>
        public override MenuItem Execute(MenuItem menuItem, DeactivateMenuItemCommand command)
        {
            ConflictGuard.ThrowIf(
                !menuItem.IsActive,
                "Menu item is already inactive"
            );

            menuItem.IsActive = false;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
