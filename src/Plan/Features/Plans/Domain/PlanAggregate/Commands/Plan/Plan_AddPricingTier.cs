namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for adding a new pricing tier to a plan.
/// </summary>
/// <param name="BillingPeriod">The billing period (Monthly, Quarterly, Semester, Yearly). Must be unique within the plan.</param>
/// <param name="Amount">The price amount for this billing period.</param>
/// <param name="CurrencyCode">The currency code (e.g., "EUR").</param>
/// <param name="IsActive">Whether the pricing tier is active. Defaults to false.</param>
public record AddPricingTierCommand(
    BillingPeriod BillingPeriod,
    decimal Amount,
    string CurrencyCode,
    bool IsActive = false
);

public partial class Plan
{
    /// <summary>
    /// Command handler for adding a new pricing tier to a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// Creates a pricing tier with an empty provider configurations collection.
    /// Provider configurations are added separately via specific commands.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddPricingTier(
        PricingTier.Create pricingTierCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<AddPricingTierCommand, Plan>
    {
        public override Plan Execute(Plan plan, AddPricingTierCommand command)
        {
            // 409 - Duplicado: BillingPeriod único en el Plan
            ConflictGuard.ThrowIf(
                plan.PricingTiers.Any(t => t.BillingPeriod == command.BillingPeriod),
                $"Pricing tier with billing period '{command.BillingPeriod}' already exists in the plan"
            );

            var tier = pricingTierCreate.Execute(new CreatePricingTierCommand(
                command.BillingPeriod,
                command.Amount,
                command.CurrencyCode,
                command.IsActive
            ));

            plan._pricingTiers.Add(tier);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
