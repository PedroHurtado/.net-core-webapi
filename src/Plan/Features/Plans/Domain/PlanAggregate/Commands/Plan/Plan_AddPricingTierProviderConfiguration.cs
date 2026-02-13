namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for adding a provider configuration to a pricing tier.
/// </summary>
/// <param name="BillingPeriod">The billing period that identifies the pricing tier.</param>
/// <param name="Provider">The payment provider name (e.g., "Stripe", "Paddle").</param>
/// <param name="ExternalProductId">The product ID in the external provider system.</param>
/// <param name="ExternalPriceId">The price ID in the external provider system.</param>
/// <param name="IsActive">Whether this configuration is active. Defaults to true.</param>
public record AddPricingTierProviderConfigurationCommand(
    BillingPeriod BillingPeriod,
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);

public partial class Plan
{
    /// <summary>
    /// Command handler for adding a provider configuration to a pricing tier.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddPricingTierProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<AddPricingTierProviderConfigurationCommand, Plan>
    {
        public override Plan Execute(Plan plan, AddPricingTierProviderConfigurationCommand command)
        {
            // 404 - Pricing tier no existe
            var tier = plan.PricingTiers.FirstOrDefault(t => t.BillingPeriod == command.BillingPeriod);
            NotFoundGuard.ThrowIfNull(tier, $"Pricing tier with billing period '{command.BillingPeriod}' not found");

            // 409 - Provider duplicado en el tier
            ConflictGuard.ThrowIf(
                tier!.ProviderConfigurations.Any(c => c.Provider == command.Provider),
                $"Configuration for '{command.Provider}' already exists in pricing tier '{command.BillingPeriod}'"
            );

            var config = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                command.Provider,
                command.ExternalProductId,
                command.ExternalPriceId,
                command.IsActive
            ));

            var updatedConfigs = tier!.ProviderConfigurations.Append(config).ToList().AsReadOnly();
            var updatedTier = tier with { ProviderConfigurations = updatedConfigs };

            plan._pricingTiers.Remove(tier);
            plan._pricingTiers.Add(updatedTier);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
