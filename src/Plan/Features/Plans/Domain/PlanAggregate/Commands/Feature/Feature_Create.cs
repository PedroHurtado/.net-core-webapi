namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for creating a feature.
/// </summary>
/// <param name="Code">The unique feature code. Must be uppercase without spaces and cannot exceed 50 characters.</param>
/// <param name="Name">The feature name. Must not be empty and cannot exceed 100 characters.</param>
/// <param name="Description">The optional feature description. Cannot exceed 250 characters.</param>
/// <param name="Type">The feature type (Boolean, Limit, or Unlimited).</param>
/// <param name="Limit">The optional limit value. Required when Type is Limit, must be greater than 0.</param>
/// <param name="Unit">The optional unit of measure. Cannot exceed 50 characters.</param>
public record CreateFeatureCommand(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit = null,
    string? Unit = null
);

public partial record Feature
{
    /// <summary>
    /// Command handler for creating a new <see cref="Feature"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a feature that defines a plan characteristic or limit.
    /// Features can be Boolean (included or not), Limit (with a numeric limit), or Unlimited.
    /// </para>
    /// <para>
    /// The command validates the feature using <see cref="FeatureValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="featureValidator">The validator for feature instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Feature> featureValidator
    ) : AbstractCreateCommand<CreateFeatureCommand, Feature>
    {
        /// <summary>
        /// Executes the create feature command.
        /// </summary>
        /// <param name="command">The command containing the feature creation data.</param>
        /// <returns>A new validated <see cref="Feature"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the feature data is invalid.</exception>
        public override Feature Execute(CreateFeatureCommand command)
        {
            var feature = new Feature(
                command.Code,
                command.Name,
                command.Description,
                command.Type,
                command.Limit,
                command.Unit);

            return featureValidator.ValidateOrThrow(feature);
        }
    }
}
