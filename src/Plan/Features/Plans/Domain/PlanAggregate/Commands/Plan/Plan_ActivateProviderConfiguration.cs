namespace Plans.Features.Plans.Domain.PlanAggregate;

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
    /// If the configuration is already active, the operation is idempotent.
    /// </para>
    /// <para>
    /// Validations:
    /// - Configuration for the provider must exist.
    /// - No other active configuration for the same provider can exist.
    /// </para>
    /// </remarks>
    /// <param name="providerConfigCreate">Command to create provider configurations.</param>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class ActivateProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<ActivateProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the activate provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the provider to activate.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the provider configuration is not found.</exception>
        /// <exception cref="ConflictException">Thrown when an active configuration for the same provider already exists.</exception>
        public override Plan Execute(Plan plan, ActivateProviderConfigurationCommand command)
        {
            var existing = plan.ProviderConfigurations.FirstOrDefault(c => c.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found");

            if (existing!.IsActive)
                return plan;

            ConflictGuard.ThrowIf(
                plan.ProviderConfigurations.Any(c => c.Provider == command.Provider && c.IsActive && c != existing),
                $"Another active configuration for '{command.Provider}' already exists");

            var activated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                existing.Provider,
                existing.ExternalProductId,
                existing.ExternalPriceId,
                true));

            plan._providerConfigurations.Remove(existing);
            plan._providerConfigurations.Add(activated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
