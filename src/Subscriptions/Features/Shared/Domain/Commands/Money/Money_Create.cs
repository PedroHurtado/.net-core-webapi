namespace Subscriptions.Features.Shared.Domain.ValueObjects;

public record CreateMoneyCommand(
    decimal Amount,
    string CurrencyCode
);

public partial record Money
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Currency.Create currencyCreate,
        IValidator<Money> moneyValidator
    ) : AbstractCreateCommand<CreateMoneyCommand, Money>
    {
        public override Money Execute(CreateMoneyCommand command)
        {
            var currency = currencyCreate.Execute(new CreateCurrencyCommand(
                command.CurrencyCode));

            var money = new Money(command.Amount, currency);

            return moneyValidator.ValidateOrThrow(money);
        }
    }
}
