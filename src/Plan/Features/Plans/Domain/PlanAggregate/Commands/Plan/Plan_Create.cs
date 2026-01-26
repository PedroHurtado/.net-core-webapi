namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for creating a new plan.
/// </summary>
/// <param name="Name">The name of the plan. Required, maximum 100 characters. Must be unique in the system.</param>
/// <param name="Description">The description of the plan. Required, maximum 500 characters.</param>
/// <param name="Amount">The price amount of the plan.</param>
/// <param name="CurrencyCode">The currency code of the plan price (e.g., "EUR").</param>
/// <param name="BillingPeriod">The billing period (Monthly, Quarterly, Semester, Yearly).</param>
public record CreatePlanCommand(
    string Name,
    string Description,
    decimal Amount,
    string CurrencyCode,
    BillingPeriod BillingPeriod
);

public partial class Plan
{
    /// <summary>
    /// Command handler for creating a new <see cref="Plan"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new plan with the provided details. The plan is created
    /// as inactive by default (IsActive = false).
    /// </para>
    /// <para>
    /// The command validates the plan using <see cref="PlanValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="moneyCreate">The service to create money instances.</param>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Money.Create moneyCreate,
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
            var currency = Currency.FromCode(command.CurrencyCode);
            var price = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, currency));

            var plan = new Plan(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                Price = price,
                BillingPeriod = command.BillingPeriod,
                IsActive = false
            };

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
