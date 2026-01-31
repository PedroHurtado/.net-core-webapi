namespace Customer.Features.Menus.Domain.MenuAggregate;

public static class ActivateValidationMessages
{
    public const string MenuAlreadyActive = "Menu is already active";
    public const string MenuMustHaveAtLeastOneCategory = "Menu must have at least one category";
    public const string MenuMustHaveAtLeastOneCategoryWithItems = "Menu must have at least one category with items";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for activating a <see cref="Menu"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command activates a menu, making it visible and available for use.
    /// A menu can only be activated if it meets the following requirements:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The menu is not already active.</description></item>
    /// <item><description>The menu has at least one category.</description></item>
    /// <item><description>At least one category has items.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<Menu>
    {
        /// <summary>
        /// Executes the activate command.
        /// </summary>
        /// <param name="menu">The menu to activate.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the menu is already active.</exception>
        /// <exception cref="ValidationException">Thrown when the menu does not meet activation requirements.</exception>
        public override Menu Execute(Menu menu)
        {
            ConflictGuard.ThrowIf(
                menu.IsActive,
                ActivateValidationMessages.MenuAlreadyActive
            );

            ValidationGuard.ThrowIf(
                menu._categories.Count == 0,
                ActivateValidationMessages.MenuMustHaveAtLeastOneCategory,
                nameof(menu.Categories)
            );

            ValidationGuard.ThrowIf(
                !menu._categories.Any(c => c.Items.Count != 0),
                ActivateValidationMessages.MenuMustHaveAtLeastOneCategoryWithItems,
                nameof(menu.Categories)
            );

            menu.IsActive = true;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}