namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Command data for updating an existing plan's basic properties.
/// </summary>
/// <remarks>
/// This command only updates basic plan properties following CQRS principles.
/// To modify Features or ProviderConfigurations, use specific commands like AddFeature, RemoveFeature, etc.
/// </remarks>
/// <param name="Name">The updated name of the plan. Required, maximum 100 characters.</param>
/// <param name="Description">The updated description of the plan. Required, maximum 500 characters.</param>
/// <param name="Amount">The updated price amount of the plan.</param>
/// <param name="CurrencyCode">The updated currency code of the plan price.</param>
/// <param name="BillingPeriod">The updated billing period (Monthly, Quarterly, Semester, Yearly).</param>
public record UpdatePlanCommand(
    string Name,
    string Description,
    decimal Amount,
    string CurrencyCode,
    BillingPeriod BillingPeriod
);

public partial class Plan
{
    /// <summary>
    /// Command handler for updating an existing <see cref="Plan"/> instance's basic properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the plan's basic properties including name, description,
    /// price, and billing period.
    /// </para>
    /// <para>
    /// The command validates the plan using <see cref="PlanValidator"/> before returning.
    /// </para>
    /// <para>
    /// Collections (Features, ProviderConfigurations) are not modified by this command.
    /// Use specific commands for collection modifications following CQRS principles.
    /// </para>
    /// </remarks>
    /// <param name="moneyCreate">The service to create money instances.</param>
    /// <param name="planValidator">The validator for plan instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        Money.Create moneyCreate,
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
            var currency = Currency.FromCode(command.CurrencyCode);
            var price = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, currency));

            // Update basic properties only
            plan.Name = command.Name;
            plan.Description = command.Description;
            plan.Price = price;
            plan.BillingPeriod = command.BillingPeriod;

            return planValidator.ValidateOrThrow(plan);
        }
    }
}
