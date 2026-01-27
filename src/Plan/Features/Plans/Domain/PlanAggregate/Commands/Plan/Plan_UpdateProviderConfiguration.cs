namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating a payment provider configuration in a plan.
/// </summary>
/// <param name="Provider">The provider name to identify which configuration to update.</param>
/// <param name="ExternalProductId">The updated external product ID from the payment provider.</param>
/// <param name="ExternalPriceId">The updated external price ID from the payment provider.</param>
public record UpdateProviderConfigurationCommand(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating a provider configuration in a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates an existing provider configuration identified by the provider name.
    /// The IsActive status is preserved.
    /// </para>
    /// <para>
    /// Validations:
    /// - Configuration for the provider must exist.
    /// </para>
    /// </remarks>
    /// <param name="providerConfigCreate">Command to create provider configurations.</param>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateProviderConfiguration(
        PaymentProviderConfig.Create providerConfigCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<UpdateProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the update provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the updated configuration data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="NotFoundException">Thrown when the configuration is not found.</exception>
        public override Plan Execute(Plan plan, UpdateProviderConfigurationCommand command)
        {
            var existing = plan.ProviderConfigurations.FirstOrDefault(c => c.Provider == command.Provider);
            NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{command.Provider}' not found");

            var updated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
                command.Provider,
                command.ExternalProductId,
                command.ExternalPriceId,
                existing!.IsActive));

            plan._providerConfigurations.Remove(existing);
            plan._providerConfigurations.Add(updated);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
