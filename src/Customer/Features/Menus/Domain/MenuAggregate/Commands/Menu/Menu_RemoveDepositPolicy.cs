namespace Customer.Features.Menus.Domain.MenuAggregate;

public partial class Menu
{
    /// <summary>
    /// Command handler for removing the <see cref="DepositPolicy"/> from a menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command clears the deposit policy from the menu, meaning reservations
    /// using this menu will no longer require a deposit (unless individual menu items
    /// have their own <see cref="ValueObjects.ItemDepositOverride"/>).
    /// </para>
    /// <para>
    /// The command validates the menu after modification.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveDepositPolicy(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<Menu>
    {
        /// <summary>
        /// Executes the remove deposit policy command.
        /// </summary>
        /// <param name="menu">The menu to remove the deposit policy from.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the menu data is invalid.</exception>
        public override Menu Execute(Menu menu)
        {
            menu.DepositPolicy = null;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
