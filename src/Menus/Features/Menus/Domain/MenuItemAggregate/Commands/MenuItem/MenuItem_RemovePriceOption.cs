namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for removing a price option from a menu item.
/// </summary>
/// <param name="PortionType">The portion type to remove.</param>
public record RemovePriceOptionCommand(
    PortionType PortionType
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for removing a price option from an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes a price option identified by its portion type.
    /// </para>
    /// <para>
    /// A <see cref="KeyNotFoundException"/> is thrown if no price option with the specified portion type exists.
    /// </para>
    /// <para>
    /// A <see cref="ValidationException"/> is thrown if attempting to remove the last price option.
    /// </para>
    /// </remarks>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemovePriceOption(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<RemovePriceOptionCommand, MenuItem>
    {
        /// <summary>
        /// Executes the remove price option command.
        /// </summary>
        /// <param name="menuItem">The menu item instance containing the price option to remove.</param>
        /// <param name="command">The remove price option command containing the portion type to remove.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no price option with the specified portion type exists.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when attempting to remove the last price option.
        /// </exception>
        public override MenuItem Execute(MenuItem menuItem, RemovePriceOptionCommand command)
        {
            var existing = menuItem.PriceOptions.FirstOrDefault(p => p.PortionType == command.PortionType);
            NotFoundGuard.ThrowIfNull(existing, $"Price option with portion type '{command.PortionType}' not found");

            ValidationGuard.ThrowIf(
                menuItem.PriceOptions.Count <= 1,
                "Cannot remove last price option",
                nameof(menuItem.PriceOptions));

            menuItem._priceOptions.Remove(existing!);

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}