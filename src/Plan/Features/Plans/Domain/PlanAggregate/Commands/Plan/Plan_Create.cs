namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for creating a new plan.
/// </summary>
/// <param name="Name">The name of the plan. Required, maximum 100 characters. Must be unique in the system.</param>
/// <param name="Description">The description of the plan. Required, maximum 500 characters.</param>
/// <param name="Price">The price of the plan as a Money value object.</param>
/// <param name="BillingPeriod">The billing period (Monthly, Quarterly, Semester, Yearly).</param>
/// <param name="Features">The collection of features for this plan. At least one feature is required.</param>
/// <param name="ProviderConfigurations">The collection of payment provider configurations. At least one is required.</param>
/// <param name="IsActive">Optional flag indicating if the plan is active. Defaults to true.</param>
public record CreatePlanCommand(
    string Name,
    string Description,
    Money Price,
    BillingPeriod BillingPeriod,
    IEnumerable<Feature> Features,
    IEnumerable<PaymentProviderConfig> ProviderConfigurations,
    bool IsActive = true
);

public partial class Plan
{
    /// <summary>
    /// Command handler for creating a new <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new plan with the provided details. The plan is created
    /// as active by default (IsActive = true).
    /// </para>
    /// <para>
    /// The command validates the plan using <see cref="PlanValidator"/> before returning.
    /// </para>
    /// <para>
    /// Validations include:
    /// - Basic data: Name, Description, Price, BillingPeriod
    /// - Features: At least one feature, unique codes, valid types
    /// - Provider configurations: At least one configuration, no duplicate active providers
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Plan> planValidator
    ) : AbstractCreateCommand<CreatePlanCommand, Plan>
    {
        /// <summary>
        /// Executes the create plan command.
        /// </summary>
        /// <param name="command">The command containing the plan creation data.</param>
        /// <returns>A new validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the plan data is invalid.</exception>
        public override Plan Execute(CreatePlanCommand command)
        {
            // Validate collection invariants before creating the plan
            
            // 422 - Validación: Al menos un Feature (dato inválido: lista vacía)
            ValidationGuard.ThrowIf(
                !command.Features.Any(),
                PlanValidationMessages.AtLeastOneFeatureRequired,
                nameof(command.Features)
            );

            // 409 - Validación: No puede haber Features con el mismo Code (conflicto: duplicados)
            var featureCodes = command.Features.Select(f => f.Code).ToList();
            ConflictGuard.ThrowIf(
                featureCodes.Distinct().Count() != featureCodes.Count,
                PlanValidationMessages.DuplicateFeatureCode
            );

            // 422 - Validación: Al menos una configuración de proveedor activa (dato inválido: ninguna activa)
            ValidationGuard.ThrowIf(
                !command.ProviderConfigurations.Any(c => c.IsActive),
                PlanValidationMessages.AtLeastOneActiveProviderRequired,
                nameof(command.ProviderConfigurations)
            );

            // 409 - Validación: No puede haber dos configuraciones activas para el mismo proveedor (conflicto: duplicados)
            var activeProviders = command.ProviderConfigurations
                .Where(c => c.IsActive)
                .Select(c => c.Provider)
                .ToList();
            
            ConflictGuard.ThrowIf(
                activeProviders.Distinct().Count() != activeProviders.Count,
                PlanValidationMessages.DuplicateActiveProvider
            );

            var plan = new Plan(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                Price = command.Price,
                BillingPeriod = command.BillingPeriod,
                IsActive = command.IsActive
            };

            // Add features to the internal collection
            foreach (var feature in command.Features)
            {
                plan._features.Add(feature);
            }

            // Add provider configurations to the internal collection
            foreach (var providerConfig in command.ProviderConfigurations)
            {
                plan._providerConfigurations.Add(providerConfig);
            }

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
