namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for updating an existing category within a menu.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category to update.</param>
/// <param name="Name">The updated name of the category. Required, maximum 100 characters.</param>
/// <param name="Description">The updated description of the category. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The updated display order for sorting categories.</param>
public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder
);

public static class UpdateCategoryValidationMessages
{
    public const string CategoryNameAlreadyExists = "A category with this name already exists";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for updating an existing <see cref="MenuCategory"/> within a menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates a category's properties including name, description, and display order.
    /// Category names must remain unique within the menu (case-insensitive comparison).
    /// </para>
    /// <para>
    /// The command uses <see cref="MenuCategory.Update"/> to apply the changes and validates
    /// the menu after modification.
    /// </para>
    /// </remarks>
    /// <param name="updateCategory">The command handler for updating categories.</param>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateCategory(
        MenuCategory.Update updateCategory,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateCategoryCommand, Menu>
    {
        /// <summary>
        /// Executes the update category command.
        /// </summary>
        /// <param name="menu">The menu containing the category to update.</param>
        /// <param name="command">The command containing the updated category data.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the category is not found in the menu.</exception>
        /// <exception cref="ConflictException">Thrown when another category with the same name already exists.</exception>
        /// <exception cref="ValidationException">Thrown when the updated data is invalid.</exception>
        public override Menu Execute(Menu menu, UpdateCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, command.CategoryId);

            var duplicateName = menu._categories.Any(c =>
                c.Id != command.CategoryId &&
                c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

            ConflictGuard.ThrowIf(
                duplicateName,
                UpdateCategoryValidationMessages.CategoryNameAlreadyExists
            );

            updateCategory.Execute(category!, new UpdateCategoryDetailsCommand(
                command.Name,
                command.Description,
                command.DisplayOrder
            ));

            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
