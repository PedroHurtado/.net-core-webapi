namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Command data for creating a new deposit policy.
/// </summary>
/// <param name="DepositType">The type of deposit calculation.</param>
/// <param name="Amount">The base amount for calculations. Must be greater than zero.</param>
/// <param name="Percentage">The percentage (1-100) for PercentageOfBill type.</param>
/// <param name="MinimumBillForDeposit">Optional minimum bill threshold.</param>
/// <param name="MinimumGuestsForDeposit">Optional minimum guest count threshold.</param>
public record CreateDepositPolicyCommand(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
);

public partial record DepositPolicy
{
    /// <summary>
    /// Command handler for creating a new <see cref="DepositPolicy"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new deposit policy with the provided configuration.
    /// The policy defines how deposits are calculated for reservations.
    /// </para>
    /// <para>
    /// The command validates the deposit policy using <see cref="DepositPolicyValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="depositPolicyValidator">The validator for deposit policy instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<DepositPolicy> depositPolicyValidator
    ) : AbstractCreateCommand<CreateDepositPolicyCommand, DepositPolicy>
    {
        /// <summary>
        /// Executes the create deposit policy command.
        /// </summary>
        /// <param name="command">The command containing the deposit policy creation data.</param>
        /// <returns>A new validated <see cref="DepositPolicy"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the deposit policy data is invalid.</exception>
        public override DepositPolicy Execute(CreateDepositPolicyCommand command)
        {
            var policy = new DepositPolicy(
                command.DepositType,
                command.Amount,
                command.Percentage,
                command.MinimumBillForDeposit,
                command.MinimumGuestsForDeposit);

            return depositPolicyValidator.ValidateOrThrow(policy);
        }
    }
}
