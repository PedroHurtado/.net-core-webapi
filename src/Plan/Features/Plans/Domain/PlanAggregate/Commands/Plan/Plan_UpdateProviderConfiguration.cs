namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command for updating a payment provider configuration in an existing plan.
/// </summary>
/// <remarks>
/// This command updates a specific provider configuration identified by the provider name.
/// Following CQRS principles, this is a specific command for a single responsibility.
/// </remarks>
/// <param name="Provider">The provider name to identify which configuration to update.</param>
/// <param name="ExternalProductId">The updated external product ID from the payment provider.</param>
/// <param name="ExternalPriceId">The updated external price ID from the payment provider.</param>
/// <param name="IsActive">The updated active status for this provider configuration.</param>
public record UpdateProviderConfigurationCommand(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating a provider configuration in an existing <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates an existing provider configuration identified by the provider name.
    /// </para>
    /// <para>
    /// Validations include:
    /// - The provider configuration must exist in the plan
    /// - After update, at least one provider configuration must remain active
    /// - No duplicate active providers are allowed
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    /// <param name="createProviderConfig">The command to create a new provider configuration.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateProviderConfiguration(
        IValidator<Plan> planValidator,
        PaymentProviderConfig.Create createProviderConfig
    ) : AbstractModifyCommand<UpdateProviderConfigurationCommand, Plan>
    {
        /// <summary>
        /// Executes the update provider configuration command.
        /// </summary>
        /// <param name="plan">The plan instance to update.</param>
        /// <param name="command">The command containing the updated provider configuration data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the provider doesn't exist or validation fails.</exception>
        /// <exception cref="ConflictException">Thrown when the update would create duplicate active providers.</exception>
        public override Plan Execute(Plan plan, UpdateProviderConfigurationCommand command)
        {
            // 422 - Validación: El proveedor debe existir en el plan
            var existingConfig = plan._providerConfigurations
                .FirstOrDefault(c => c.Provider == command.Provider);

            ValidationGuard.ThrowIf(
                existingConfig == null,
                $"Provider configuration '{command.Provider}' not found in plan",
                nameof(command.Provider)
            );

            // Remove the old configuration
            plan._providerConfigurations.Remove(existingConfig!);

            // Create the updated configuration using the Create command
            var updatedConfig = createProviderConfig.Execute(
                new CreatePaymentProviderConfigCommand(
                    command.Provider,
                    command.ExternalProductId,
                    command.ExternalPriceId,
                    command.IsActive
                )
            );

            // Add the updated configuration
            plan._providerConfigurations.Add(updatedConfig);

            // 422 - Validación: Al menos una configuración de proveedor activa
            ValidationGuard.ThrowIf(
                !plan._providerConfigurations.Any(c => c.IsActive),
                PlanValidationMessages.AtLeastOneActiveProviderRequired,
                nameof(command.IsActive)
            );

            // 409 - Validación: No puede haber dos configuraciones activas para el mismo proveedor
            var activeProviders = plan._providerConfigurations
                .Where(c => c.IsActive)
                .Select(c => c.Provider)
                .ToList();

            ConflictGuard.ThrowIf(
                activeProviders.Distinct().Count() != activeProviders.Count,
                PlanValidationMessages.DuplicateActiveProvider
            );

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
