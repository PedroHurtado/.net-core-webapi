namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command for deactivating an existing plan.
/// </summary>
/// <remarks>
/// This command sets the plan's IsActive property to false.
/// Following CQRS principles, this is a specific command for a single responsibility.
/// </remarks>
public record DeactivatePlanCommand();

public partial class Plan
{
    /// <summary>
    /// Command handler for deactivating an existing <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets the plan's IsActive property to false.
    /// </para>
    /// <para>
    /// If the plan is already inactive, a ConflictException is thrown to prevent
    /// redundant operations.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<DeactivatePlanCommand, Plan>
    {
        /// <summary>
        /// Executes the deactivate plan command.
        /// </summary>
        /// <param name="plan">The plan instance to deactivate.</param>
        /// <param name="command">The deactivate command (empty).</param>
        /// <returns>The deactivated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ConflictException">Thrown when the plan is already inactive.</exception>
        public override Plan Execute(Plan plan, DeactivatePlanCommand command)
        {
            // 409 - Validación: No se puede desactivar un plan que ya está inactivo
            ConflictGuard.ThrowIf(
                !plan.IsActive,
                "Cannot deactivate a plan that is already inactive"
            );

            plan.IsActive = false;

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
