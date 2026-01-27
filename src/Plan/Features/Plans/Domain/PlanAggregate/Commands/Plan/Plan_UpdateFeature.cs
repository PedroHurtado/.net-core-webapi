namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating an existing feature in a plan.
/// </summary>
/// <param name="Code">The unique code of the feature to update. Cannot be changed.</param>
/// <param name="Name">The new display name of the feature.</param>
/// <param name="Description">The new description of the feature.</param>
/// <param name="Type">The new type of feature.</param>
/// <param name="Limit">The new limit value. Required if Type is Limit, must be null otherwise.</param>
/// <param name="Unit">The new unit of measurement.</param>
public record UpdateFeatureCommand(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit = null,
    string? Unit = null
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating an existing feature in a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates an existing feature in the plan's collection.
    /// The feature is identified by its Code.
    /// </para>
    /// <para>
    /// Validations:
    /// - Feature must exist in the plan.
    /// - Feature type rules (Limit > 0 for Limit type, null for others).
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateFeature(
        Feature.Create featureCreate,
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<UpdateFeatureCommand, Plan>
    {
        /// <summary>
        /// Executes the update feature command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the updated feature data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the feature data is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the feature is not found.</exception>
        public override Plan Execute(Plan plan, UpdateFeatureCommand command)
        {
            // Find existing feature
            var existingFeature = plan.Features.FirstOrDefault(f => f.Code == command.Code);
            
            // 404 - Validation: Feature must exist
            NotFoundGuard.ThrowIfNull(
                existingFeature,
                $"Feature with code '{command.Code}' not found in the plan"
            );

            // Create new feature instance using the service
            var newFeature = featureCreate.Execute(new CreateFeatureCommand(
                command.Code,
                command.Name,
                command.Description,
                command.Type,
                command.Type == FeatureType.Limit ? command.Limit : null,
                command.Unit
            ));

            // Replace in collection (remove old, add new)
            plan._features.Remove(existingFeature!);
            plan._features.Add(newFeature);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
