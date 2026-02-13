namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for activating a pricing tier within a plan.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier to activate.</param>
public record ActivatePricingTierCommand(BillingPeriod BillingPeriod);

public partial class Plan
{
    /// <summary>
    /// Command handler for activating a pricing tier within a <see cref="Plan"/>.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class ActivatePricingTier(
        PricingTier.Activate pricingTierActivate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<ActivatePricingTierCommand, Plan>
    {
        public override Plan Execute(Plan plan, ActivatePricingTierCommand command)
        {
            // 404 - No existe
            var existing = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(existing, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 409 - Ya activo
            ConflictGuard.ThrowIf(
                existing!.IsActive,
                $"Pricing tier with billing period '{command.BillingPeriod}' is already active"
            );

            var updated = pricingTierActivate.Execute(existing!);

            plan._pricingTiers.Remove(existing!);
            plan._pricingTiers.Add(updated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
