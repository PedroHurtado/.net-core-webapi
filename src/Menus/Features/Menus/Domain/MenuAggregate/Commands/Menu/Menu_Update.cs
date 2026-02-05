namespace Menus.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for updating an existing menu's properties.
/// </summary>
/// <param name="Name">The updated name of the menu. Required, maximum 100 characters.</param>
/// <param name="Description">The updated description of the menu. Maximum 500 characters.</param>
/// <param name="EffectiveFrom">The updated start date for the menu's validity period.</param>
/// <param name="EffectiveUntil">The updated end date for the menu's validity period.</param>
/// <param name="DisplayOrder">The updated display order for sorting menus.</param>
public record UpdateMenuCommand(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    int DisplayOrder
);

public partial class Menu
{
    /// <summary>
    /// Command handler for updating an existing <see cref="Menu"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the menu's basic properties including name, description,
    /// validity dates, and display order.
    /// </para>
    /// <para>
    /// The command validates the menu using <see cref="MenuValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        /// <summary>
        /// Executes the update menu command.
        /// </summary>
        /// <param name="menu">The menu instance to update.</param>
        /// <param name="command">The command containing the updated menu data.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the updated data is invalid.</exception>
        public override Menu Execute(Menu menu, UpdateMenuCommand command)
        {
            menu.Name = command.Name;
            menu.Description = command.Description;
            menu.EffectiveFrom = command.EffectiveFrom;
            menu.EffectiveUntil = command.EffectiveUntil;
            menu.DisplayOrder = command.DisplayOrder;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
