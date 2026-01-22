using FluentValidation;
using Fudie.DependencyInjection;
using Fudie.Domain;
using Fudie.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for creating a currency.
/// </summary>
/// <param name="Code">The ISO 4217 currency code. Must be exactly 3 uppercase characters.</param>
/// <param name="Symbol">The currency symbol. Must not be empty and cannot exceed 5 characters.</param>
/// <param name="DecimalPlaces">The number of decimal places for this currency. Must be between 0 and 4. Defaults to 2.</param>
public record CreateCurrencyCommand(
    string Code,
    string Symbol,
    int DecimalPlaces = 2
);

public partial record Currency
{
    /// <summary>
    /// Command handler for creating a new <see cref="Currency"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a currency with an ISO 4217 code, symbol, and decimal places.
    /// </para>
    /// <para>
    /// The command validates the currency using <see cref="CurrencyValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="currencyValidator">The validator for currency instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Currency> currencyValidator
    ) : AbstractCreateCommand<CreateCurrencyCommand, Currency>
    {
        /// <summary>
        /// Executes the create currency command.
        /// </summary>
        /// <param name="command">The command containing the currency creation data.</param>
        /// <returns>A new validated <see cref="Currency"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the currency data is invalid.</exception>
        public override Currency Execute(CreateCurrencyCommand command)
        {
            var currency = new Currency(
                command.Code,
                command.Symbol,
                command.DecimalPlaces);

            return currencyValidator.ValidateOrThrow(currency);
        }
    }
}
