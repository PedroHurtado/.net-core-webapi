namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for deactivating a pricing tier within a plan.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier to deactivate.</param>
public record DeactivatePricingTierCommand(BillingPeriod BillingPeriod);

public partial class Plan
{
    /// <summary>
    /// Command handler for deactivating a pricing tier within a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// Cannot deactivate the last active pricing tier with an active provider from an active plan.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class DeactivatePricingTier(
        PricingTier.Deactivate pricingTierDeactivate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<DeactivatePricingTierCommand, Plan>
    {
        public override Plan Execute(Plan plan, DeactivatePricingTierCommand command)
        {
            // 404 - No existe
            var existing = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(existing, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 422 - No se puede desactivar si Plan activo y es el último tier activo con provider activo
            var activeTiersWithProvider = plan.PricingTiers.Count(t => t.IsActive && t.HasActiveProvider);
            var isLastActiveTierWithProvider = existing!.IsActive && existing!.HasActiveProvider && activeTiersWithProvider <= 1;

            ValidationGuard.ThrowIf(
                plan.IsActive && isLastActiveTierWithProvider,
                "Cannot deactivate the last active pricing tier with an active provider from an active plan",
                nameof(plan.PricingTiers)
            );

            var updated = pricingTierDeactivate.Execute(existing!);

            plan._pricingTiers.Remove(existing!);
            plan._pricingTiers.Add(updated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
