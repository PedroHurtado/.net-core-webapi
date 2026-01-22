namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for creating money.
/// </summary>
/// <param name="Amount">The monetary amount. Must be greater than or equal to zero.</param>
/// <param name="Currency">The currency. Must not be null.</param>
public record CreateMoneyCommand(
    decimal Amount,
    Currency Currency
);

public partial record Money
{
    /// <summary>
    /// Command handler for creating a new <see cref="Money"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a money value object that combines an amount with a currency.
    /// </para>
    /// <para>
    /// The command validates the money using <see cref="MoneyValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="moneyValidator">The validator for money instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Money> moneyValidator
    ) : AbstractCreateCommand<CreateMoneyCommand, Money>
    {
        /// <summary>
        /// Executes the create money command.
        /// </summary>
        /// <param name="command">The command containing the money creation data.</param>
        /// <returns>A new validated <see cref="Money"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the money data is invalid.</exception>
        public override Money Execute(CreateMoneyCommand command)
        {
            var money = new Money(
                command.Amount,
                command.Currency);

            return moneyValidator.ValidateOrThrow(money);
        }
    }
}
