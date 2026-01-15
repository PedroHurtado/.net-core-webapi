namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for creating a new menu item.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant that will own the menu item.</param>
/// <param name="Name">The name of the menu item. Required, maximum 150 characters.</param>
/// <param name="PriceOptions">The price options for the menu item. At least one is required.</param>
/// <param name="Description">Optional description of the menu item. Maximum 1000 characters.</param>
/// <param name="ImageUrl">Optional image URL. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The display order. Defaults to 0.</param>
/// <param name="IsHighRiskItem">Whether this is a high-risk item. Defaults to false.</param>
/// <param name="RequiresAdvanceOrder">Whether this item requires advance ordering. Defaults to false.</param>
/// <param name="MinimumAdvanceOrderQuantity">The minimum quantity for advance orders. Must be between 1 and 100 if specified.</param>
/// <param name="IsAlwaysAvailable">Whether the item is available every day. Defaults to true.</param>
/// <param name="AvailableDays">The days when the item is available. Required if IsAlwaysAvailable is false.</param>
/// <param name="Allergens">Optional collection of allergens associated with this item.</param>
/// <param name="AllergenNotes">Optional allergen notes. Maximum 500 characters.</param>
/// <param name="DepositOverride">Optional deposit override configuration.</param>
/// <param name="NutritionalInfo">Optional nutritional information.</param>
public record CreateMenuItemCommand(
    Guid TenantId,
    string Name,
    HashSet<PriceOption> PriceOptions,
    string? Description = null,
    string? ImageUrl = null,
    int DisplayOrder = 0,
    bool IsHighRiskItem = false,
    bool RequiresAdvanceOrder = false,
    int? MinimumAdvanceOrderQuantity = null,
    bool IsAlwaysAvailable = true,
    HashSet<DayOfWeek>? AvailableDays = null,
    HashSet<Allergen>? Allergens = null,
    string? AllergenNotes = null,
    ItemDepositOverride? DepositOverride = null,
    NutritionalInfo? NutritionalInfo = null
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for creating a new <see cref="MenuItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new menu item with the provided details. The item is created
    /// as active and available by default.
    /// </para>
    /// <para>
    /// The command validates the menu item using <see cref="MenuItemValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractCreateCommand<CreateMenuItemCommand, MenuItem>
    {
        /// <summary>
        /// Executes the create menu item command.
        /// </summary>
        /// <param name="command">The command containing the menu item creation data.</param>
        /// <returns>A new validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the menu item data is invalid.</exception>
        public override MenuItem Execute(CreateMenuItemCommand command)
        {
            var menuItem = new MenuItem(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = command.Name,
                Description = command.Description,
                ImageUrl = command.ImageUrl,
                DisplayOrder = command.DisplayOrder,
                IsActive = true,
                IsHighRiskItem = command.IsHighRiskItem,
                RequiresAdvanceOrder = command.RequiresAdvanceOrder,
                MinimumAdvanceOrderQuantity = command.MinimumAdvanceOrderQuantity,
                IsAvailable = true,
                IsAlwaysAvailable = command.IsAlwaysAvailable,
                DepositOverride = command.DepositOverride,
                NutritionalInfo = command.NutritionalInfo,
                AllergenNotes = command.AllergenNotes
            };

            foreach (var priceOption in command.PriceOptions)
            {
                menuItem._priceOptions.Add(priceOption);
            }

            if (command.AvailableDays != null)
            {
                foreach (var day in command.AvailableDays)
                {
                    menuItem._availableDays.Add(day);
                }
            }

            if (command.Allergens != null)
            {
                foreach (var allergen in command.Allergens)
                {
                    menuItem._allergens.Add(allergen);
                }
            }

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}