namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for removing a feature from a plan.
/// </summary>
/// <param name="Code">The unique code of the feature to remove.</param>
public record RemoveFeatureCommand(
    string Code
);

public partial class Plan
{
    /// <summary>
    /// Command handler for removing a feature from a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command removes a feature from the plan's collection.
    /// </para>
    /// <para>
    /// Validations:
    /// - Feature must exist in the plan.
    /// - Plan must have at least one feature remaining after removal.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveFeature(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<RemoveFeatureCommand, Plan>
    {
        /// <summary>
        /// Executes the remove feature command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the feature code to remove.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the removal violates plan invariants.</exception>
        /// <exception cref="NotFoundException">Thrown when the feature is not found.</exception>
        public override Plan Execute(Plan plan, RemoveFeatureCommand command)
        {
            var existing = plan.Features.FirstOrDefault(f => f.Code == command.Code);
            NotFoundGuard.ThrowIfNull(existing, $"Feature with code '{command.Code}' not found");

            ValidationGuard.ThrowIf(
                plan.IsActive && plan.Features.Count <= 1,
                "Cannot remove last feature from active plan",
                nameof(plan.Features));

            plan._features.Remove(existing!);

            return planValidator.ValidateOrThrow(plan);

        }
    }
}
