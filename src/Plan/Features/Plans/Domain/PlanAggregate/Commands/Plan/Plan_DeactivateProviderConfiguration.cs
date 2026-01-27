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
    /// If the configuration is already inactive, the operation is idempotent.
    /// </para>
    /// <para>
    /// Validations:
    /// - Configuration for the provider must exist.
    /// - If the plan is active and this is the only active configuration, deactivation is not allowed.
    /// </para>
    /// </remarks>
    /// <param name="providerConfigCreate">Command to create provider configurations.</param>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class DeactivateProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
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
            var existing = plan.ProviderConfigurations.FirstOrDefault(p => p.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found");

            if (!existing!.IsActive)
                return plan;

            var activeConfigsCount = plan.ProviderConfigurations.Count(p => p.IsActive);
            ValidationGuard.ThrowIf(
                plan.IsActive && activeConfigsCount <= 1,
                "Plan must have at least one active provider configuration",
                nameof(plan.ProviderConfigurations));

            var deactivated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                existing.Provider,
                existing.ExternalProductId,
                existing.ExternalPriceId,
                false));

            plan._providerConfigurations.Remove(existing);
            plan._providerConfigurations.Add(deactivated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
