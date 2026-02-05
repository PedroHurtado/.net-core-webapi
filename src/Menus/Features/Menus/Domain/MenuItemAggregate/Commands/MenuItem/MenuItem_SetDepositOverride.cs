namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for setting a deposit override on a menu item.
/// </summary>
/// <param name="DepositAmount">The fixed deposit amount. Must be greater than zero.</param>
/// <param name="MinimumQuantityForDeposit">Optional minimum quantity threshold (must be at least 1 if specified).</param>
public record SetDepositOverrideCommand(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit = null
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for setting a deposit override on an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets or replaces the deposit override on a menu item.
    /// The deposit override supersedes the menu's default deposit policy.
    /// </para>
    /// <para>
    /// This is an idempotent operation - setting a deposit override when one already exists
    /// will replace the existing override.
    /// </para>
    /// </remarks>
    /// <param name="itemDepositOverrideCreate">The command handler for creating deposit overrides.</param>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class SetDepositOverride(
        ItemDepositOverride.Create itemDepositOverrideCreate,
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<SetDepositOverrideCommand, MenuItem>
    {
        /// <summary>
        /// Executes the set deposit override command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to set the deposit override on.</param>
        /// <param name="command">The command containing the deposit override data.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the deposit override data is invalid.</exception>
        public override MenuItem Execute(MenuItem menuItem, SetDepositOverrideCommand command)
        {
            var depositOverride = itemDepositOverrideCreate.Execute(
                new CreateItemDepositOverrideCommand(
                    command.DepositAmount,
                    command.MinimumQuantityForDeposit
                )
            );

            menuItem.DepositOverride = depositOverride;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
