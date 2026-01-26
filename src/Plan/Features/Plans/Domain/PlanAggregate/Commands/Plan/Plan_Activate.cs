namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command for activating an existing plan.
/// </summary>
/// <remarks>
/// This command sets the plan's IsActive property to true.
/// Following CQRS principles, this is a specific command for a single responsibility.
/// </remarks>
public record ActivatePlanCommand();

public partial class Plan
{
    /// <summary>
    /// Command handler for activating an existing <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the plan's IsActive property to true.
    /// </para>
    /// <para>
    /// If the plan is already active, a ConflictException is thrown to prevent
    /// redundant operations.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<ActivatePlanCommand, Plan>
    {
        /// <summary>
        /// Executes the activate plan command.
        /// </summary>
        /// <param name="plan">The plan instance to activate.</param>
        /// <param name="command">The activate command (empty).</param>
        /// <returns>The activated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the plan is already active.</exception>
        public override Plan Execute(Plan plan, ActivatePlanCommand command)
        {
            // 409 - Validación: No se puede activar un plan que ya está activo
            ConflictGuard.ThrowIf(
                plan.IsActive,
                "Cannot activate a plan that is already active"
            );

            // 422 - Validación: Al menos un Feature (dato inválido: lista vacía)
            ValidationGuard.ThrowIf(
                !plan.Features.Any(),
                PlanValidationMessages.AtLeastOneFeatureRequired,
                nameof(plan.Features)
            );

            // 422 - Validación: Al menos una configuración de proveedor activa (dato inválido: ninguna activa)
            ValidationGuard.ThrowIf(
                !plan.ProviderConfigurations.Any(c => c.IsActive),
                PlanValidationMessages.AtLeastOneActiveProviderRequired,
                nameof(plan.ProviderConfigurations)
            );

            plan.IsActive = true;

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
