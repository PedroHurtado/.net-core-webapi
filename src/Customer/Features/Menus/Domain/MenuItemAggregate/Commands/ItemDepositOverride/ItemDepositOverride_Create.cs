namespace Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

/// <summary>
/// Command data for creating an item deposit override.
/// </summary>
/// <param name="DepositAmount">The fixed deposit amount. Must be greater than zero.</param>
/// <param name="MinimumQuantityForDeposit">Optional minimum quantity threshold (must be at least 1 if specified).</param>
public record CreateItemDepositOverrideCommand(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit = null
);

public partial record ItemDepositOverride
{
    /// <summary>
    /// Command handler for creating a new <see cref="ItemDepositOverride"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a deposit override for a specific menu item that supersedes
    /// the menu's default deposit policy.
    /// </para>
    /// <para>
    /// The command validates the item deposit override using <see cref="ItemDepositOverrideValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="itemDepositOverrideValidator">The validator for item deposit override instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ItemDepositOverride> itemDepositOverrideValidator
    ) : AbstractCreateCommand<CreateItemDepositOverrideCommand, ItemDepositOverride>
    {
        /// <summary>
        /// Executes the create item deposit override command.
        /// </summary>
        /// <param name="command">The command containing the item deposit override creation data.</param>
        /// <returns>A new validated <see cref="ItemDepositOverride"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the item deposit override data is invalid.</exception>
        public override ItemDepositOverride Execute(CreateItemDepositOverrideCommand command)
        {
            var itemOverride = new ItemDepositOverride(
                command.DepositAmount,
                command.MinimumQuantityForDeposit);

            return itemDepositOverrideValidator.ValidateOrThrow(itemOverride);
        }
    }
}
