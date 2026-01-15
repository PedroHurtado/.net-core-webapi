namespace Customer.Features.Menus.Domain.Shared.ValueObjects;

/// <summary>
/// Command data for creating a price option.
/// </summary>
/// <param name="PortionType">The portion type.</param>
/// <param name="Price">The price amount. Required for fixed portion types; optional for MarketPrice.</param>
/// <param name="IsActive">Whether the option is active. Defaults to true.</param>
public record CreatePriceOptionCommand(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);

public partial record PriceOption
{
    /// <summary>
    /// Command handler for creating a new <see cref="PriceOption"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a pricing option for a specific portion type of a menu item.
    /// </para>
    /// <para>
    /// The command validates the price option using <see cref="PriceOptionValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="priceOptionValidator">The validator for price option instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<PriceOption> priceOptionValidator
    ) : AbstractCreateCommand<CreatePriceOptionCommand, PriceOption>
    {
        /// <summary>
        /// Executes the create price option command.
        /// </summary>
        /// <param name="command">The command containing the price option creation data.</param>
        /// <returns>A new validated <see cref="PriceOption"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the price option data is invalid.</exception>
        public override PriceOption Execute(CreatePriceOptionCommand command)
        {
            var option = new PriceOption(
                command.PortionType,
                command.Price,
                command.IsActive);

            return priceOptionValidator.ValidateOrThrow(option);
        }
    }
}
