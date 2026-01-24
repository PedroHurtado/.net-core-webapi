namespace Plan.Features.Plans.Domain.PlanAggregate;

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
            // Find existing feature
            var existingFeature = plan.Features.FirstOrDefault(f => f.Code == command.Code);
            
            // 404 - Validation: Feature must exist
            NotFoundGuard.ThrowIfNull(
                existingFeature,
                $"Feature with code '{command.Code}' not found in the plan"
            );

            // 422 - Validation: At least one feature must remain
            // We check if this is the last feature
            ValidationGuard.ThrowIf(
                plan.Features.Count <= 1,
                PlanValidationMessages.AtLeastOneFeatureRequired,
                nameof(plan.Features)
            );

            // Remove from collection
            plan._features.Remove(existingFeature!);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
