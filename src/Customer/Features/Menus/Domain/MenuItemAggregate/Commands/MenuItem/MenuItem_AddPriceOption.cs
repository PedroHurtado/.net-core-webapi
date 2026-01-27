namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for adding a price option to a menu item.
/// </summary>
/// <param name="PortionType">The portion type (Small, Half, Full, or MarketPrice).</param>
/// <param name="Price">The price amount. Required for fixed portion types; optional for MarketPrice.</param>
/// <param name="IsActive">Whether the option is active. Defaults to true.</param>
public record AddPriceOptionCommand(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for adding a price option to an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a new price option to a menu item's collection of price options.
    /// </para>
    /// <para>
    /// A <see cref="ConflictException"/> is thrown if a price option with the same portion type already exists.
    /// </para>
    /// </remarks>
    /// <param name="priceOptionCreate">The command for creating price option value objects.</param>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddPriceOption(
        PriceOption.Create priceOptionCreate,
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<AddPriceOptionCommand, MenuItem>
    {
        /// <summary>
        /// Executes the add price option command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to add the price option to.</param>
        /// <param name="command">The add price option command containing the price option data.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="ConflictException">
        /// Thrown when a price option with the same portion type already exists.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when the price option data is invalid.
        /// </exception>
        public override MenuItem Execute(MenuItem menuItem, AddPriceOptionCommand command)
        {
            ConflictGuard.ThrowIf(
                menuItem.PriceOptions.Any(p => p.PortionType == command.PortionType),
                $"Price option with portion type '{command.PortionType}' already exists");

            var priceOption = priceOptionCreate.Execute(
                new CreatePriceOptionCommand(
                    command.PortionType,
                    command.Price,
                    command.IsActive
                )
            );

            menuItem._priceOptions.Add(priceOption);

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
