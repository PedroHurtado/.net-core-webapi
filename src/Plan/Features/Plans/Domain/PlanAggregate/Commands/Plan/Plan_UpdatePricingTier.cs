namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating the price of an existing pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier to update.</param>
/// <param name="Amount">The new price amount.</param>
/// <param name="CurrencyCode">The new currency code (e.g., "EUR").</param>
public record UpdatePricingTierCommand(
    BillingPeriod BillingPeriod,
    decimal Amount,
    string CurrencyCode
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating the price of an existing pricing tier.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="PricingTier.UpdatePrice"/> transform to modify only the price.
    /// Preserves IsActive and ProviderConfigurations.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePricingTier(
        PricingTier.UpdatePrice pricingTierUpdatePrice,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<UpdatePricingTierCommand, Plan>
    {
        public override Plan Execute(Plan plan, UpdatePricingTierCommand command)
        {
            // 404 - No existe
            var existing = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(existing, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            var updated = pricingTierUpdatePrice.Execute(existing!, new UpdatePricePricingTierCommand(
                command.Amount,
                command.CurrencyCode
            ));

            plan._pricingTiers.Remove(existing!);
            plan._pricingTiers.Add(updated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
