namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for creating a new menu item.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant that will own the menu item.</param>
/// <param name="Name">The name of the menu item. Required, maximum 100 characters.</param>
/// <param name="Description">Optional description of the menu item. Maximum 1000 characters.</param>
/// <param name="ImageUrl">Optional image URL. Maximum 500 characters.</param>
/// <param name="DisplayOrder">The display order. Defaults to 0.</param>
/// <param name="IsHighRiskItem">Whether this is a high-risk item. Defaults to false.</param>
/// <param name="RequiresAdvanceOrder">Whether this item requires advance ordering. Defaults to false.</param>
/// <param name="MinimumAdvanceOrderQuantity">The minimum quantity for advance orders. Must be between 1 and 100 if specified.</param>
/// <param name="IsAlwaysAvailable">Whether the item is available every day. Defaults to true.</param>
/// <param name="AvailableDays">The days when the item is available. Required if IsAlwaysAvailable is false.</param>
/// <param name="AllergenNotes">Optional allergen notes. Maximum 500 characters.</param>
/// <param name="PriceOptions">The price options for the menu item. At least one is required.</param>
public record CreateMenuItemCommand(
    Guid TenantId,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    DayOfWeek[] AvailableDays,
    string? AllergenNotes,
    CreatePriceOptionCommand[] PriceOptions
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for creating a new <see cref="MenuItem"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new menu item with the provided details. The item is created
    /// as inactive (IsActive=false) and available (IsAvailable=true) by default.
    /// </para>
    /// <para>
    /// The command validates the menu item using <see cref="MenuItemValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="priceOptionCreate">The command handler for creating price options.</param>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        PriceOption.Create priceOptionCreate,
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
            var priceOptions = command.PriceOptions
                .Select(po => priceOptionCreate.Execute(po))
                .ToList();

            var menuItem = new MenuItem(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = command.Name,
                Description = command.Description,
                ImageUrl = command.ImageUrl,
                DisplayOrder = command.DisplayOrder,
                IsActive = false,
                IsAvailable = true,
                IsHighRiskItem = command.IsHighRiskItem,
                RequiresAdvanceOrder = command.RequiresAdvanceOrder,
                MinimumAdvanceOrderQuantity = command.MinimumAdvanceOrderQuantity,
                IsAlwaysAvailable = command.IsAlwaysAvailable,
                AllergenNotes = command.AllergenNotes,
                DepositOverride = null,
                NutritionalInfo = null
            };

            foreach (var priceOption in priceOptions)
            {
                menuItem._priceOptions.Add(priceOption);
            }

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
