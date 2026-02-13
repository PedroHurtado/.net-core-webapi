namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for creating a new pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period (Monthly, Quarterly, Semester, Yearly).</param>
/// <param name="Amount">The price amount for this billing period.</param>
/// <param name="CurrencyCode">The currency code (e.g., "EUR").</param>
/// <param name="IsActive">Whether the pricing tier is active. Defaults to false.</param>
public record CreatePricingTierCommand(
    BillingPeriod BillingPeriod,
    decimal Amount,
    string CurrencyCode,
    bool IsActive = false
);

public partial record PricingTier
{
    /// <summary>
    /// Command handler for creating a new <see cref="PricingTier"/> instance.
    /// </summary>
    /// <remarks>
    /// Creates a pricing tier with an empty provider configurations collection.
    /// Provider configurations are added separately via Plan aggregate commands.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Money.Create moneyCreate,
        IValidator<PricingTier> pricingTierValidator
    ) : AbstractCreateCommand<CreatePricingTierCommand, PricingTier>
    {
        public override PricingTier Execute(CreatePricingTierCommand command)
        {
            var currency = Currency.FromCode(command.CurrencyCode);
            var money = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, currency));

            var tier = new PricingTier(
                command.BillingPeriod,
                money,
                command.IsActive,
                Array.Empty<PaymentProviderConfig>().AsReadOnly());

            return pricingTierValidator.ValidateOrThrow(tier);
        }
    }
}
