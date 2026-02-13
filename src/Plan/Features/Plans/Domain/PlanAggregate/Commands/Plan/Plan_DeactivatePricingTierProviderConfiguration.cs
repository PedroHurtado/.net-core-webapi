namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for deactivating a provider configuration within a pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier.</param>
/// <param name="Provider">The provider name to deactivate.</param>
public record DeactivatePricingTierProviderConfigurationCommand(
    BillingPeriod BillingPeriod,
    string Provider
);

public partial class Plan
{
    /// <summary>
    /// Command handler for deactivating a provider configuration within a pricing tier.
    /// </summary>
    /// <remarks>
    /// Cannot deactivate the last active provider of the last active pricing tier in an active plan.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class DeactivatePricingTierProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<DeactivatePricingTierProviderConfigurationCommand, Plan>
    {
        public override Plan Execute(Plan plan, DeactivatePricingTierProviderConfigurationCommand command)
        {
            // 404 - Pricing tier no existe
            var tier = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(tier, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 404 - Config no existe
            var existing = tier!.ProviderConfigurations.FirstOrDefault(c => c.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found in pricing tier '{command.BillingPeriod}'");

            // 409 - Ya inactiva
            ConflictGuard.ThrowIf(
                !existing!.IsActive,
                $"Configuration for '{command.Provider}' is already inactive in pricing tier '{command.BillingPeriod}'"
            );

            // 422 - No se puede desactivar si es el último provider activo del último tier activo de un plan activo
            if (plan.IsActive)
            {
                var activeProvidersInTier = tier!.ProviderConfigurations.Count(c => c.IsActive);
                var isLastActiveProviderInTier = activeProvidersInTier <= 1;
                var otherActiveTiersWithProvider = plan.PricingTiers
                    .Where(t => t.BillingPeriod != command.BillingPeriod)
                    .Any(t => t.IsActive && t.HasActiveProvider);

                ValidationGuard.ThrowIf(
                    isLastActiveProviderInTier && tier!.IsActive && !otherActiveTiersWithProvider,
                    "Cannot deactivate the last active provider configuration from the last active pricing tier of an active plan",
                    nameof(tier.ProviderConfigurations)
                );
            }

            var deactivated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                existing!.Provider,
                existing!.ExternalProductId,
                existing!.ExternalPriceId,
                false
            ));

            var updatedConfigs = tier!.ProviderConfigurations
                .Where(c => c.Provider != command.Provider)
                .Append(deactivated)
                .ToList()
                .AsReadOnly();

            var updatedTier = tier with { ProviderConfigurations = updatedConfigs };

            plan._pricingTiers.Remove(tier);
            plan._pricingTiers.Add(updatedTier);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
