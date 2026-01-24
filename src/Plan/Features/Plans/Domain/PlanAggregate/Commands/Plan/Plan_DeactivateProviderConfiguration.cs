namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for deactivating a payment provider configuration in a plan.
/// </summary>
/// <param name="Provider">The name of the payment provider to deactivate.</param>
public record DeactivateProviderConfigurationCommand(
    string Provider
);

public partial class Plan
{
    /// <summary>
    /// Command handler for deactivating a payment provider configuration in a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command deactivates an existing provider configuration.
    /// </para>
    /// <para>
    /// Validations:
    /// - Configuration for the provider must exist and be active.
    /// - Plan must have at least one active configuration remaining after deactivation.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class DeactivateProviderConfiguration(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<DeactivateProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the deactivate provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the provider to deactivate.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the deactivation violates plan invariants.</exception>
        /// <exception cref="NotFoundException">Thrown when the provider configuration is not found.</exception>
        public override Plan Execute(Plan plan, DeactivateProviderConfigurationCommand command)
        {
            // Find existing config
            var existingConfig = plan.ProviderConfigurations.FirstOrDefault(p => p.Provider == command.Provider);

            // 404 - Validation: Config must exist
            NotFoundGuard.ThrowIfNull(
                existingConfig,
                $"Configuration for provider '{command.Provider}' not found in the plan"
            );

            // If already inactive, nothing to do (idempotent) or throw? 
            // Usually idempotent is better, but if we want to be strict about "Deactivate" implies it was active...
            // Let's assume if it's found, we proceed.

            // 422 - Validation: At least one active configuration must remain
            // We check if this is the ONLY active configuration
            var activeConfigsCount = plan.ProviderConfigurations.Count(p => p.IsActive);
            
            if (existingConfig.IsActive)
            {
                ValidationGuard.ThrowIf(
                    activeConfigsCount <= 1,
                    "Plan must have at least one active provider configuration",
                    nameof(plan.ProviderConfigurations)
                );
            }
            else
            {
                // If it's already inactive, we can just return the plan or throw.
                // Let's just return.
                return plan;
            }

            // Create new inactive config using derived record to access protected constructor
            var newConfig = new CreatablePaymentProviderConfig(
                existingConfig.Provider,
                existingConfig.ExternalProductId,
                existingConfig.ExternalPriceId,
                false // Set to inactive
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
