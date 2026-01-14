namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for creating a new menu.
/// </summary>
/// <param name="RestaurantId">The unique identifier of the restaurant that will own the menu.</param>
/// <param name="Name">The name of the menu. Required, maximum 100 characters.</param>
/// <param name="Description">Optional description of the menu. Maximum 500 characters.</param>
/// <param name="EffectiveFrom">Optional start date for the menu's validity period.</param>
/// <param name="EffectiveUntil">Optional end date for the menu's validity period.</param>
public record CreateMenuCommand(
    Guid RestaurantId,
    string Name,
    string? Description = null,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveUntil = null
);

public partial class Menu
{
    /// <summary>
    /// Command handler for creating a new <see cref="Menu"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new menu with the provided details. The menu is created
    /// as active by default with a display order of 0.
    /// </para>
    /// <para>
    /// The command validates the menu using <see cref="MenuValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Menu> menuValidator
    ) : AbstractCreateCommand<CreateMenuCommand, Menu>
    {
        /// <summary>
        /// Executes the create menu command.
        /// </summary>
        /// <param name="command">The command containing the menu creation data.</param>
        /// <returns>A new validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the menu data is invalid.</exception>
        public override Menu Execute(CreateMenuCommand command)
        {
            var menu = new Menu(Guid.NewGuid())
            {
                RestaurantId = command.RestaurantId,
                Name = command.Name,
                Description = command.Description,
                EffectiveFrom = command.EffectiveFrom,
                EffectiveUntil = command.EffectiveUntil,
                DisplayOrder = 0,
                IsActive = true
            };

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
