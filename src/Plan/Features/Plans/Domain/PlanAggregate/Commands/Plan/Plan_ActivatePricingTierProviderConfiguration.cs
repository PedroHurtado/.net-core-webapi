namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for activating a provider configuration within a pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier.</param>
/// <param name="Provider">The provider name to activate.</param>
public record ActivatePricingTierProviderConfigurationCommand(
    BillingPeriod BillingPeriod,
    string Provider
);

public partial class Plan
{
    /// <summary>
    /// Command handler for activating a provider configuration within a pricing tier.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class ActivatePricingTierProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<ActivatePricingTierProviderConfigurationCommand, Plan>
    {
        public override Plan Execute(Plan plan, ActivatePricingTierProviderConfigurationCommand command)
        {
            // 404 - Pricing tier no existe
            var tier = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(tier, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 404 - Config no existe
            var existing = tier!.ProviderConfigurations.FirstOrDefault(c => c.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found in pricing tier '{command.BillingPeriod}'");

            // 409 - Ya activa
            ConflictGuard.ThrowIf(
                existing!.IsActive,
                $"Configuration for '{command.Provider}' is already active in pricing tier '{command.BillingPeriod}'"
            );

            var activated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                existing!.Provider,
                existing!.ExternalProductId,
                existing!.ExternalPriceId,
                true
            ));

            var updatedConfigs = tier!.ProviderConfigurations
                .Where(c => c.Provider != command.Provider)
                .Append(activated)
                .ToList()
                .AsReadOnly();

            var updatedTier = tier with { ProviderConfigurations = updatedConfigs };

            plan._pricingTiers.Remove(tier);
            plan._pricingTiers.Add(updatedTier);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
