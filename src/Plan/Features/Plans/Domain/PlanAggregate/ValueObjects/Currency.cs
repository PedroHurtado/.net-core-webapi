

namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Represents a currency according to ISO 4217 standard.
/// </summary>
/// <remarks>
/// <para>
/// This value object defines a currency with its ISO code, symbol, and decimal places.
/// It provides predefined instances for common currencies (EUR, USD, GBP).
/// </para>
/// <para>
/// The decimal places property determines how many decimal digits should be used
/// when displaying or calculating amounts in this currency.
/// </para>
/// </remarks>
public partial record Currency
{
    /// <summary>
    /// Gets the ISO 4217 currency code.
    /// </summary>
    /// <value>A three-letter uppercase currency code (e.g., "EUR", "USD", "GBP").</value>
    public string Code { get; }

    /// <summary>
    /// Gets the currency symbol.
    /// </summary>
    /// <value>The symbol used to represent the currency (e.g., "€", "$", "£").</value>
    public string Symbol { get; }

    /// <summary>
    /// Gets the number of decimal places for this currency.
    /// </summary>
    /// <value>
    /// The number of decimal digits (typically 2 for most currencies, 0 for JPY).
    /// Must be between 0 and 4.
    /// </value>
    public int DecimalPlaces { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> record.
    /// </summary>
    /// <param name="code">The ISO 4217 currency code.</param>
    /// <param name="symbol">The currency symbol.</param>
    /// <param name="decimalPlaces">The number of decimal places.</param>
    protected Currency(
        string code,
        string symbol,
        int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    /// <summary>
    /// Gets the Euro currency (EUR).
    /// </summary>
    /// <value>A currency instance for EUR with symbol "€" and 2 decimal places.</value>
    public static Currency EUR => new("EUR", "€", 2);

    /// <summary>
    /// Gets the US Dollar currency (USD).
    /// </summary>
    /// <value>A currency instance for USD with symbol "$" and 2 decimal places.</value>
    public static Currency USD => new("USD", "$", 2);

    /// <summary>
    /// Gets the British Pound currency (GBP).
    /// </summary>
    /// <value>A currency instance for GBP with symbol "£" and 2 decimal places.</value>
    public static Currency GBP => new("GBP", "£", 2);

    /// <summary>
    /// Creates a currency instance from an ISO 4217 currency code.
    /// </summary>
    /// <param name="code">The ISO 4217 currency code (case-insensitive).</param>
    /// <returns>A currency instance for the specified code.</returns>
    /// <exception cref="ValidationException">Thrown when the currency code is not supported.</exception>
    public static Currency FromCode(string code) => code.ToUpper() switch
    {
        "EUR" => EUR,
        "USD" => USD,
        "GBP" => GBP,
        _ => throw new ValidationException([new ValidationFailure("CurrencyCode", $"Currency {code} not supported")])
    };
}

public static class CurrencyValidationMessages
{
    public const string CodeRequired = "Currency code is required";
    public const string CodeLength = "Currency code must be exactly 3 characters";
    public const string CodeUppercase = "Currency code must be uppercase";
    public const string SymbolRequired = "Currency symbol is required";
    public const string SymbolMaxLength = "Currency symbol cannot exceed 5 characters";
    public const string DecimalPlacesRange = "Decimal places must be between 0 and 4";
}

/// <summary>
/// Provides validation rules for the <see cref="Currency"/> value object.
/// </summary>
/// <remarks>
/// Validates currency invariants including ISO 4217 code format,
/// symbol requirements, and decimal places constraints.
/// </remarks>
public class CurrencyValidator : AbstractValidator<Currency>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public CurrencyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(CurrencyValidationMessages.CodeRequired);

        RuleFor(x => x.Code)
            .Length(3)
            .WithMessage(CurrencyValidationMessages.CodeLength);

        RuleFor(x => x.Code)
            .Must(code => code == code.ToUpper())
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(CurrencyValidationMessages.CodeUppercase);

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage(CurrencyValidationMessages.SymbolRequired);

        RuleFor(x => x.Symbol)
            .MaximumLength(5)
            .WithMessage(CurrencyValidationMessages.SymbolMaxLength);

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 4)
            .WithMessage(CurrencyValidationMessages.DecimalPlacesRange);
    }
}
