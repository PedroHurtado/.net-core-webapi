namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for updating the price of a pricing tier.
/// </summary>
/// <param name="Amount">The new price amount.</param>
/// <param name="CurrencyCode">The new currency code (e.g., "EUR").</param>
public record UpdatePricePricingTierCommand(
    decimal Amount,
    string CurrencyCode
);

public partial record PricingTier
{
    /// <summary>
    /// Command handler for updating the price of a <see cref="PricingTier"/> instance.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePrice(
        Money.Create moneyCreate,
        IValidator<PricingTier> pricingTierValidator
    ) : AbstractTransformCommand<UpdatePricePricingTierCommand, PricingTier>
    {
        public override PricingTier Execute(PricingTier current, UpdatePricePricingTierCommand command)
        {
            var currency = Currency.FromCode(command.CurrencyCode);
            var money = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, currency));

            var updated = current with { Price = money };

            return pricingTierValidator.ValidateOrThrow(updated);
        }
    }
}
