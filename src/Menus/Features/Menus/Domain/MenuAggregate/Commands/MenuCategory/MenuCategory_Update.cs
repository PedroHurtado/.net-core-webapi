namespace Menus.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Command data for updating an existing menu category's details.
/// </summary>
/// <param name="Name">The updated name of the category. Required, maximum 100 characters.</param>
/// <param name="Description">The updated description of the category. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The updated display order for sorting categories.</param>
public record UpdateCategoryDetailsCommand(
    string Name,
    string? Description,
    int DisplayOrder
);

public partial class MenuCategory
{
    /// <summary>
    /// Command handler for updating an existing <see cref="MenuCategory"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the category's basic properties including name, description,
    /// and display order.
    /// </para>
    /// <para>
    /// Note: This command is typically invoked through <see cref="Menu.UpdateCategory"/> which
    /// handles uniqueness validation within the menu context.
    /// </para>
    /// <para>
    /// The command validates the category using <see cref="MenuCategoryValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="categoryValidator">The validator for category instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<MenuCategory> categoryValidator
    ) : AbstractModifyCommand<UpdateCategoryDetailsCommand, MenuCategory>
    {
        /// <summary>
        /// Executes the update category command.
        /// </summary>
        /// <param name="category">The category instance to update.</param>
        /// <param name="command">The command containing the updated category data.</param>
        /// <returns>The updated and validated <see cref="MenuCategory"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the updated data is invalid.</exception>
        public override MenuCategory Execute(MenuCategory category, UpdateCategoryDetailsCommand command)
        {
            category.Name = command.Name;
            category.Description = command.Description;
            category.DisplayOrder = command.DisplayOrder;

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
