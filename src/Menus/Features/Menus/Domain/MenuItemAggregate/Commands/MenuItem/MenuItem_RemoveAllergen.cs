namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for removing an allergen from a menu item.
/// </summary>
/// <param name="AllergenId">The allergen identifier to remove.</param>
public record RemoveAllergenCommand(
    string AllergenId
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for removing an allergen from an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes an allergen identified by its identifier.
    /// </para>
    /// <para>
    /// A <see cref="KeyNotFoundException"/> is thrown if no allergen with the specified identifier exists.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveAllergen(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<RemoveAllergenCommand, MenuItem>
    {
        /// <summary>
        /// Executes the remove allergen command.
        /// </summary>
        /// <param name="menuItem">The menu item instance containing the allergen to remove.</param>
        /// <param name="command">The remove allergen command containing the allergen identifier to remove.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no allergen with the specified identifier exists.
        /// </exception>
        public override MenuItem Execute(MenuItem menuItem, RemoveAllergenCommand command)
        {
            var existing = menuItem.Allergens.FirstOrDefault(a => a.Id == command.AllergenId);
            NotFoundGuard.ThrowIfNull(existing, "Allergen not found in this item");

            menuItem._allergens.Remove(existing!);

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
