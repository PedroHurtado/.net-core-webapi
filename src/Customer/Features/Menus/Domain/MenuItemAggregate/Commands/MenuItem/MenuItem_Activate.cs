namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for activating an existing menu item.
/// </summary>
/// <remarks>
/// This command sets the menu item's IsActive property to true.
/// Following CQRS principles, this is a specific command for a single responsibility.
/// </remarks>
public record ActivateMenuItemCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for activating an existing <see cref="MenuItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the menu item's IsActive property to true.
    /// </para>
    /// <para>
    /// If the menu item is already active, a ConflictException is thrown.
    /// If the menu item has no active price options, a ValidationException is thrown.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<ActivateMenuItemCommand, MenuItem>
    {
        /// <summary>
        /// Executes the activate menu item command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to activate.</param>
        /// <param name="command">The activate command (empty).</param>
        /// <returns>The activated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the menu item is already active.</exception>
        /// <exception cref="ValidationException">Thrown when the menu item has no active price options.</exception>
        public override MenuItem Execute(MenuItem menuItem, ActivateMenuItemCommand command)
        {
            ConflictGuard.ThrowIf(
                menuItem.IsActive,
                "Menu item is already active"
            );

            ValidationGuard.ThrowIf(
                !menuItem.HasActivePriceOption,
                "Menu item must have at least one active price option",
                nameof(menuItem.PriceOptions)
            );

            menuItem.IsActive = true;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
