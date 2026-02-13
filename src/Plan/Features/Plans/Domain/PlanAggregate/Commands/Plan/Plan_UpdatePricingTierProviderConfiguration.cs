namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating a provider configuration within a pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier.</param>
/// <param name="Provider">The provider name to identify which configuration to update.</param>
/// <param name="ExternalProductId">The updated external product ID.</param>
/// <param name="ExternalPriceId">The updated external price ID.</param>
public record UpdatePricingTierProviderConfigurationCommand(
    BillingPeriod BillingPeriod,
    string Provider,
    string ExternalProductId,
    string ExternalPriceId
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating a provider configuration within a pricing tier.
    /// </summary>
    /// <remarks>
    /// Preserves the IsActive status of the existing configuration.
    /// </remarks>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePricingTierProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<UpdatePricingTierProviderConfigurationCommand, Plan>
    {
        public override Plan Execute(Plan plan, UpdatePricingTierProviderConfigurationCommand command)
        {
            // 404 - Pricing tier no existe
            var tier = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(tier, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 404 - Config no existe
            var existing = tier!.ProviderConfigurations.FirstOrDefault(c => c.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found in pricing tier '{command.BillingPeriod}'");

            var updated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                command.Provider,
                command.ExternalProductId,
                command.ExternalPriceId,
                existing!.IsActive
            ));

            var updatedConfigs = tier!.ProviderConfigurations
                .Where(c => c.Provider != command.Provider)
                .Append(updated)
                .ToList()
                .AsReadOnly();

            var updatedTier = tier with { ProviderConfigurations = updatedConfigs };

            plan._pricingTiers.Remove(tier);
            plan._pricingTiers.Add(updatedTier);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
