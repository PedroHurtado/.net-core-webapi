namespace Menus.Features.Menus.Domain.MenuAggregate;

public static class DeactivateValidationMessages
{
    public const string MenuAlreadyInactive = "Menu is already inactive";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for deactivating a <see cref="Menu"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command deactivates a menu, making it no longer visible or available for use.
    /// A menu can only be deactivated if it is currently active.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<Menu>
    {
        /// <summary>
        /// Executes the deactivate command.
        /// </summary>
        /// <param name="menu">The menu to deactivate.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the menu is already inactive.</exception>
        public override Menu Execute(Menu menu)
        {
            ConflictGuard.ThrowIf(
                !menu.IsActive,
                DeactivateValidationMessages.MenuAlreadyInactive
            );

            menu.IsActive = false;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}