namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for adding a new feature to a plan.
/// </summary>
/// <param name="Code">The unique code of the feature (e.g., "RESERVATIONS_MONTHLY"). Required, uppercase, no spaces.</param>
/// <param name="Name">The display name of the feature. Required.</param>
/// <param name="Description">Optional description of the feature.</param>
/// <param name="Type">The type of feature (Boolean, Limit, Unlimited).</param>
/// <param name="Limit">The numeric limit value. Required if Type is Limit, must be null otherwise.</param>
/// <param name="Unit">The unit of measurement (e.g., "users", "seats"). Optional.</param>
public record AddFeatureCommand(
    string Code,
    string Name,
    string Description,
    FeatureType Type,
    int Limit,
    string Unit
);

public partial class Plan
{
    /// <summary>
    /// Command handler for adding a new feature to a <see cref="Plan"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command adds a new feature to the plan's collection.
    /// </para>
    /// <para>
    /// Validations:
    /// - Feature code must be unique within the plan.
    /// - Feature type rules (Limit > 0 for Limit type, null for others).
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class AddFeature(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<AddFeatureCommand, Plan>
    {
        /// <summary>
        /// Executes the add feature command.
        /// </summary>
        /// <param name="plan">The plan instance to modify.</param>
        /// <param name="command">The command containing the new feature data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the feature data is invalid.</exception>
        /// <exception cref="ConflictException">Thrown when a feature with the same code already exists.</exception>
        public override Plan Execute(Plan plan, AddFeatureCommand command)
        {
            // 409 - Validation: Feature Code Unique
            ConflictGuard.ThrowIf(
                plan.Features.Any(f => f.Code == command.Code),
                $"Feature with code '{command.Code}' already exists in the plan"
            );

            // Create the new feature value object
            // The Feature record constructor or factory should handle basic validations, 
            // but we can also validate specific business rules here if needed.
            
            // Validate Feature Type rules specific to this flow if not covered by Feature VO
            if (command.Type == FeatureType.Limit)
            {
                ValidationGuard.ThrowIf(
                    command.Limit <= 0,
                    "Feature of type Limit must have a limit value greater than 0",
                    nameof(command.Limit)
                );
            }
            else // Boolean or Unlimited
            {
                // We don't throw here if Limit has value because the command forces an int value.
                // Instead, we just ignore it when creating the Feature (pass null).
                // However, if we want to be strict that the client should send 0 or something specific, we could validte.
                // But usually, we just ensure we don't create an invalid Feature.
            }

            var feature = Feature.New(
                command.Code,
                command.Name,
                command.Description,
                command.Type,
                command.Type == FeatureType.Limit ? command.Limit : null,
                command.Unit
            );

            // Add to internal collection
            plan._features.Add(feature);

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
