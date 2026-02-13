namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for removing a pricing tier from a plan.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier to remove.</param>
public record RemovePricingTierCommand(BillingPeriod BillingPeriod);

public partial class Plan
{
    /// <summary>
    /// Command handler for removing a pricing tier from a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// Cannot remove the last active pricing tier with an active provider from an active plan.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemovePricingTier(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<RemovePricingTierCommand, Plan>
    {
        public override Plan Execute(Plan plan, RemovePricingTierCommand command)
        {
            // 404 - No existe
            var existing = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(existing, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 422 - No se puede eliminar si Plan activo y es el último tier activo con provider activo
            var activeTiersWithProvider = plan.PricingTiers.Count(t => t.IsActive && t.HasActiveProvider);
            var isLastActiveTierWithProvider = existing!.IsActive && existing!.HasActiveProvider && activeTiersWithProvider <= 1;

            ValidationGuard.ThrowIf(
                plan.IsActive && isLastActiveTierWithProvider,
                "Cannot remove the last active pricing tier with an active provider from an active plan",
                nameof(plan.PricingTiers)
            );

            plan._pricingTiers.Remove(existing!);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
