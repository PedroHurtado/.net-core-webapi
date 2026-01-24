namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for adding a new payment provider configuration to a plan.
/// </summary>
/// <param name="Provider">The name of the payment provider (e.g., "Stripe", "Paddle"). Required.</param>
/// <param name="ExternalProductId">The product ID in the external provider system. Required.</param>
/// <param name="ExternalPriceId">The price ID in the external provider system. Required.</param>
/// <param name="IsActive">Whether this configuration is active. Defaults to true.</param>
public record AddProviderConfigurationCommand(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);

public partial class Plan
{
    /// <summary>
    /// Command handler for adding a new payment provider configuration to a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a new provider configuration to the plan's collection.
    /// </para>
    /// <para>
    /// Validations:
    /// - No other active configuration for the same provider can exist.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddProviderConfiguration(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<AddProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the add provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the new configuration data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the configuration data is invalid.</exception>
        /// <exception cref="ConflictException">Thrown when an active configuration for the same provider already exists.</exception>
        public override Plan Execute(Plan plan, AddProviderConfigurationCommand command)
        {
            // 409 - Validation: No duplicate active provider
            if (command.IsActive)
            {
                ConflictGuard.ThrowIf(
                    plan.ProviderConfigurations.Any(p => p.Provider == command.Provider && p.IsActive),
                    $"An active configuration for provider '{command.Provider}' already exists in the plan"
                );
            }

            // Create using derived record to access protected constructor
            var config = new CreatablePaymentProviderConfig(
                command.Provider,
                command.ExternalProductId,
                command.ExternalPriceId,
                command.IsActive
            );

            // Validate the config explicitly since PlanValidator doesn't validate the collection
            new PaymentProviderConfigValidator().ValidateOrThrow(config);

            // Add to internal collection
            plan._providerConfigurations.Add(config);

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
