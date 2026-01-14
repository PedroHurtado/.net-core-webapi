namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for adding a new category to a menu.
/// </summary>
/// <param name="Name">The name of the category. Required, maximum 100 characters.</param>
/// <param name="Description">Optional description of the category. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The display order for sorting categories. Defaults to 0.</param>
public record AddCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

public static class AddCategoryValidationMessages
{
    public const string CategoryNameAlreadyExists = "A category with this name already exists";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for adding a new <see cref="MenuCategory"/> to a menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a new category to the menu's collection. Category names must be
    /// unique within the menu (case-insensitive comparison).
    /// </para>
    /// <para>
    /// The command uses <see cref="MenuCategory.Create"/> to instantiate the category
    /// and validates the menu after modification.
    /// </para>
    /// </remarks>
    /// <param name="createCategory">The command handler for creating categories.</param>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddCategory(
        MenuCategory.Create createCategory,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<AddCategoryCommand, Menu>
    {
        /// <summary>
        /// Executes the add category command.
        /// </summary>
        /// <param name="menu">The menu to add the category to.</param>
        /// <param name="command">The command containing the category data.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when a category with the same name already exists.</exception>
        /// <exception cref="ValidationException">Thrown when the category or menu data is invalid.</exception>
        public override Menu Execute(Menu menu, AddCategoryCommand command)
        {
            var duplicateName = menu._categories.Any(c => c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

            ConflictGuard.ThrowIf(
                duplicateName,
                AddCategoryValidationMessages.CategoryNameAlreadyExists
            );

            var category = createCategory.Execute(new CreateCategoryCommand(
                command.Name,
                command.Description,
                command.DisplayOrder
            ));

            menu._categories.Add(category);

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
