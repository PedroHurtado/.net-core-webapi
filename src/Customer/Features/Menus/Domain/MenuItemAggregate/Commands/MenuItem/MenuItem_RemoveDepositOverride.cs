namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for removing a deposit override from a menu item.
/// </summary>
/// <remarks>
/// This command removes the deposit override, reverting to the menu's default deposit policy.
/// This is an idempotent operation - calling it on an item without a deposit override has no effect.
/// </remarks>
public record RemoveDepositOverrideCommand();

public partial class MenuItem
{
    /// <summary>
    /// Command handler for removing a deposit override from an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes the deposit override from a menu item, causing it to use
    /// the menu's default deposit policy instead.
    /// </para>
    /// <para>
    /// This is an idempotent operation - calling it multiple times has the same effect as calling it once.
    /// No conflict exception is thrown if the item doesn't have a deposit override.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveDepositOverride(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<RemoveDepositOverrideCommand, MenuItem>
    {
        /// <summary>
        /// Executes the remove deposit override command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to remove the deposit override from.</param>
        /// <param name="command">The remove deposit override command (empty).</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        public override MenuItem Execute(MenuItem menuItem, RemoveDepositOverrideCommand command)
        {
            menuItem.DepositOverride = null;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
