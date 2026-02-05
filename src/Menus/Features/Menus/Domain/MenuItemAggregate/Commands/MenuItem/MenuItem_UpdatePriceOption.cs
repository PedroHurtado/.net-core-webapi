namespace Menus.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for updating a price option on a menu item.
/// </summary>
/// <param name="PortionType">The portion type to update.</param>
/// <param name="Price">The new price amount. Required for fixed portion types; optional for MarketPrice.</param>
/// <param name="IsActive">Whether the option should be active.</param>
public record UpdatePriceOptionCommand(
    PortionType PortionType,
    decimal? Price,
    bool IsActive
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for updating a price option on an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates an existing price option identified by its portion type.
    /// </para>
    /// <para>
    /// A <see cref="NotFoundException"/> is thrown if no price option with the specified portion type exists.
    /// </para>
    /// <para>
    /// A <see cref="ValidationException"/> is thrown if attempting to deactivate the last active price option.
    /// </para>
    /// </remarks>
    /// <param name="priceOptionCreate">The command for creating price option value objects.</param>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePriceOption(
        PriceOption.Create priceOptionCreate,
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<UpdatePriceOptionCommand, MenuItem>
    {
        /// <summary>
        /// Executes the update price option command.
        /// </summary>
        /// <param name="menuItem">The menu item instance containing the price option to update.</param>
        /// <param name="command">The update price option command containing the new values.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        /// <exception cref="NotFoundException">
        /// Thrown when no price option with the specified portion type exists.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when attempting to deactivate the last active price option or when the price option data is invalid.
        /// </exception>
        public override MenuItem Execute(MenuItem menuItem, UpdatePriceOptionCommand command)
        {
            var existing = menuItem.PriceOptions.FirstOrDefault(p => p.PortionType == command.PortionType);
            NotFoundGuard.ThrowIfNull(existing, $"Price option with portion type '{command.PortionType}' not found");

            ValidationGuard.ThrowIf(
                !command.IsActive && menuItem.PriceOptions.Count(p => p.IsActive) <= 1 && existing!.IsActive,
                "Cannot deactivate last active price option",
                nameof(menuItem.PriceOptions));

            var updated = priceOptionCreate.Execute(
                new CreatePriceOptionCommand(
                    command.PortionType,
                    command.Price,
                    command.IsActive
                )
            );

            menuItem._priceOptions.Remove(existing!);
            menuItem._priceOptions.Add(updated);

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
