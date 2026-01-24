namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for activating a payment provider configuration in a plan.
/// </summary>
/// <param name="Provider">The name of the payment provider to activate.</param>
public record ActivateProviderConfigurationCommand(
    string Provider
);

public partial class Plan
{
    /// <summary>
    /// Command handler for activating a payment provider configuration in a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command activates an existing provider configuration.
    /// </para>
    /// <para>
    /// Validations:
    /// - Configuration for the provider must exist.
    /// - No other active configuration for the same provider can exist (should be impossible if we only have one config per provider, but good to check).
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class ActivateProviderConfiguration(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<ActivateProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the activate provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the provider to activate.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the activation violates plan invariants.</exception>
        /// <exception cref="NotFoundException">Thrown when the provider configuration is not found.</exception>
        /// <exception cref="ConflictException">Thrown when an active configuration for the same provider already exists.</exception>
        public override Plan Execute(Plan plan, ActivateProviderConfigurationCommand command)
        {
            // Find existing config
            var existingConfig = plan.ProviderConfigurations.FirstOrDefault(p => p.Provider == command.Provider);

            // 404 - Validation: Config must exist
            NotFoundGuard.ThrowIfNull(
                existingConfig,
                $"Configuration for provider '{command.Provider}' not found in the plan"
            );

            // If already active, nothing to do (idempotent)
            if (existingConfig.IsActive)
            {
                return plan;
            }

            // 409 - Validation: No duplicate active provider
            // Since we found existingConfig and it is inactive, we check if there are ANY active configs for this provider.
            // But wait, existingConfig IS the config for this provider. If there were another one, it would be a duplicate provider.
            // Our invariant says "No puede haber dos configuraciones para el mismo proveedor ambas activas".
            // Actually, our invariant says "No puede haber dos configuraciones para el mismo proveedor" (implied by unique provider name in list?).
            // Let's check Plan.md: "No puede haber dos configuraciones para el mismo proveedor ambas activas".
            // But usually we key by Provider name. If we allow multiple configs for same provider (e.g. one active, one inactive), then we need to check.
            // My implementation of AddProviderConfiguration allows multiple configs for same provider if one is inactive.
            
            // So, check if there is ALREADY an active config for this provider (different from the one we are activating)
            if (plan.ProviderConfigurations.Any(p => p.Provider == command.Provider && p.IsActive && p != existingConfig))
            {
                throw new ConflictException($"An active configuration for provider '{command.Provider}' already exists in the plan");
            }

            // Create new active config using derived record to access protected constructor
            var newConfig = new CreatablePaymentProviderConfig(
                existingConfig.Provider,
                existingConfig.ExternalProductId,
                existingConfig.ExternalPriceId,
                true // Set to active
            );

            // Replace in collection
            plan._providerConfigurations.Remove(existingConfig);
            plan._providerConfigurations.Add(newConfig);

            return planValidator.ValidateOrThrow(plan);
        }

        // Helper record to access protected constructor
        private record CreatablePaymentProviderConfig(
            string Provider,
            string ExternalProductId,
            string ExternalPriceId,
            bool IsActive
        ) : PaymentProviderConfig(Provider, ExternalProductId, ExternalPriceId, IsActive);
    }
}
