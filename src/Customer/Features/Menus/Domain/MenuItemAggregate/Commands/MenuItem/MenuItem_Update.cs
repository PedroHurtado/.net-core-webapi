namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for updating an existing menu item.
/// </summary>
/// <param name="Name">The name of the menu item. Required, maximum 100 characters.</param>
/// <param name="Description">Optional description of the menu item. Maximum 1000 characters.</param>
/// <param name="ImageUrl">Optional image URL. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The display order. Must be greater than or equal to 0.</param>
/// <param name="IsHighRiskItem">Whether this is a high-risk item.</param>
/// <param name="RequiresAdvanceOrder">Whether this item requires advance ordering.</param>
/// <param name="MinimumAdvanceOrderQuantity">The minimum quantity for advance orders. Must be between 1 and 100 if specified.</param>
/// <param name="IsAlwaysAvailable">Whether the item is available every day.</param>
/// <param name="AvailableDays">The days when the item is available. Required if IsAlwaysAvailable is false.</param>
/// <param name="AllergenNotes">Optional allergen notes. Maximum 500 characters.</param>
public record UpdateMenuItemCommand(
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    DayOfWeek[] AvailableDays,
    string? AllergenNotes
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for updating an existing <see cref="MenuItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the menu item with the provided details. It updates all editable
    /// properties including name, description, availability settings, and allergen notes.
    /// </para>
    /// <para>
    /// The command validates the menu item using <see cref="MenuItemValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<UpdateMenuItemCommand, MenuItem>
    {
        /// <summary>
        /// Executes the update menu item command.
        /// </summary>
        /// <param name="menuItem">The existing menu item to update.</param>
        /// <param name="command">The command containing the menu item update data.</param>
        /// <returns>The updated validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the menu item data is invalid.</exception>
        public override MenuItem Execute(MenuItem menuItem, UpdateMenuItemCommand command)
        {
            menuItem.Name = command.Name;
            menuItem.Description = command.Description;
            menuItem.ImageUrl = command.ImageUrl;
            menuItem.DisplayOrder = command.DisplayOrder;
            menuItem.IsHighRiskItem = command.IsHighRiskItem;
            menuItem.RequiresAdvanceOrder = command.RequiresAdvanceOrder;
            menuItem.MinimumAdvanceOrderQuantity = command.MinimumAdvanceOrderQuantity;
            menuItem.IsAlwaysAvailable = command.IsAlwaysAvailable;
            menuItem.AllergenNotes = command.AllergenNotes;

            menuItem._availableDays.Clear();
            if (!command.IsAlwaysAvailable)
            {
                foreach (var day in command.AvailableDays)
                {
                    menuItem._availableDays.Add(day);
                }
            }

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}