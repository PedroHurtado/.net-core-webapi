namespace Menus.Features.Menus.Domain.MenuAggregate;

public record UpdatePriceOptionData(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);

/// <summary>
/// Command data for updating an item within a category.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category.</param>
/// <param name="MenuItemId">The unique identifier of the menu item to update.</param>
/// <param name="DisplayOrder">The new display order within the category.</param>
/// <param name="PriceOverrides">Optional new price overrides data for this category context.</param>
public record UpdateCategoryItemCommand(
    Guid CategoryId,
    Guid MenuItemId,
    int DisplayOrder,
    UpdatePriceOptionData[]? PriceOverrides
);

public static class UpdateCategoryItemValidationMessages
{
    public const string CategoryNotFound = "Category not found";
}

public partial class Menu
{
    /// <summary>
    /// Command handler for updating a <see cref="CategoryItem"/> within a <see cref="MenuCategory"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the display order and price overrides for an existing item
    /// in a category. It delegates to <see cref="MenuCategory.UpdateItem"/> for the actual
    /// item modification.
    /// </para>
    /// <para>
    /// The command validates that the category exists before delegating the update.
    /// </para>
    /// </remarks>
    /// <param name="updateItem">The command handler for updating items in categories.</param>
    /// <param name="menuValidator">The validator for menu instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateCategoryItem(
        MenuCategory.UpdateItem updateItem,
        PriceOption.Create createPriceOption,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateCategoryItemCommand, Menu>
    {
        /// <summary>
        /// Executes the update category item command.
        /// </summary>
        /// <param name="menu">The menu containing the category and item.</param>
        /// <param name="command">The command containing the updated item data.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the category or item is not found.</exception>
        /// <exception cref="ValidationException">Thrown when the item data is invalid.</exception>
        public override Menu Execute(Menu menu, UpdateCategoryItemCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, UpdateCategoryItemValidationMessages.CategoryNotFound);

            var priceOverrides = command.PriceOverrides?
                .Select(p => createPriceOption.Execute(new CreatePriceOptionCommand(p.PortionType, p.Price, p.IsActive)))
                .ToHashSet();

            updateItem.Execute(category!, new UpdateItemCommand(
                command.MenuItemId,
                command.DisplayOrder,
                priceOverrides
            ));

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
