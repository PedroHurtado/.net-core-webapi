namespace Menus.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// Command data for creating a new menu category.
/// </summary>
/// <param name="Name">The name of the category. Required, maximum 100 characters.</param>
/// <param name="Description">Optional description of the category. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The display order for sorting categories. Defaults to 0.</param>
public record CreateCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

public partial class MenuCategory
{
    /// <summary>
    /// Command handler for creating a new <see cref="MenuCategory"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new category with the provided details. The category is created
    /// as active by default.
    /// </para>
    /// <para>
    /// Note: This command is typically invoked through <see cref="Menu.AddCategory"/> which
    /// handles uniqueness validation within the menu context.
    /// </para>
    /// <para>
    /// The command validates the category using <see cref="MenuCategoryValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="categoryValidator">The validator for category instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<MenuCategory> categoryValidator
    ) : AbstractCreateCommand<CreateCategoryCommand, MenuCategory>
    {
        /// <summary>
        /// Executes the create category command.
        /// </summary>
        /// <param name="command">The command containing the category creation data.</param>
        /// <returns>A new validated <see cref="MenuCategory"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the category data is invalid.</exception>
        public override MenuCategory Execute(CreateCategoryCommand command)
        {
            var category = new MenuCategory(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                DisplayOrder = command.DisplayOrder,
                IsActive = true
            };

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
