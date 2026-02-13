namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for creating a new plan.
/// </summary>
/// <param name="Name">The name of the plan. Required, maximum 100 characters. Must be unique in the system.</param>
/// <param name="Description">The description of the plan. Required, maximum 500 characters.</param>
public record CreatePlanCommand(
    string Name,
    string Description
);

public partial class Plan
{
    /// <summary>
    /// Command handler for creating a new <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new plan with the provided details. The plan is created
    /// as inactive by default (IsActive = false) with no pricing tiers.
    /// </para>
    /// <para>
    /// Pricing tiers are added separately via the AddPricingTier command.
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
            var plan = new Plan(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                IsActive = false
            };

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
