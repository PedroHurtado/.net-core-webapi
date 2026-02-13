namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating an existing plan's basic properties.
/// </summary>
/// <remarks>
/// This command only updates basic plan properties following CQRS principles.
/// To modify Features or PricingTiers, use specific commands like AddFeature, AddPricingTier, etc.
/// </remarks>
/// <param name="Name">The updated name of the plan. Required, maximum 100 characters.</param>
/// <param name="Description">The updated description of the plan. Required, maximum 500 characters.</param>
public record UpdatePlanCommand(
    string Name,
    string Description
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating an existing <see cref="Plan"/> instance's basic properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the plan's basic properties: name and description.
    /// </para>
    /// <para>
    /// The command validates the plan using <see cref="PlanValidator"/> before returning.
    /// </para>
    /// <para>
    /// Collections (Features, PricingTiers) are not modified by this command.
    /// Use specific commands for collection modifications following CQRS principles.
    /// </para>
    /// </remarks>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Plan> planValidator
    ) : AbstractModifyCommand<UpdatePlanCommand, Plan>
    {
        /// <summary>
        /// Executes the update plan command.
        /// </summary>
        /// <param name="plan">The plan instance to update.</param>
        /// <param name="command">The command containing the updated plan data.</param>
        /// <returns>The updated and validated <see cref="Plan"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the updated data is invalid.</exception>
        public override Plan Execute(Plan plan, UpdatePlanCommand command)
        {
            plan.Name = command.Name;
            plan.Description = command.Description;

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
