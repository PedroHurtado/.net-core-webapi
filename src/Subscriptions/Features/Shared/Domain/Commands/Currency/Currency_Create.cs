namespace Subscriptions.Features.Shared.Domain.ValueObjects;

public record CreateCurrencyCommand(
    string CurrencyCode
);

public partial record Currency
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Currency> currencyValidator
    ) : AbstractCreateCommand<CreateCurrencyCommand, Currency>
    {
        public override Currency Execute(CreateCurrencyCommand command)
        {
            var currency = FromCode(command.CurrencyCode);

            return currencyValidator.ValidateOrThrow(currency);
        }
    }
}
