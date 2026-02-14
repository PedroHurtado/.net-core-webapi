namespace Subscriptions.Features.Shared.Domain.ValueObjects;

/// <summary>
/// Represents a monetary currency with its display format.
/// </summary>
/// <remarks>
/// <para>Defines the currency code (ISO 4217), display symbol, and decimal precision.</para>
/// <para>Includes static instances for commonly supported currencies.</para>
/// </remarks>
public partial record Currency(
    string Code,
    string Symbol,
    int DecimalPlaces
)
{
    public static readonly Currency EUR = new("EUR", "€", 2);
    public static readonly Currency USD = new("USD", "$", 2);
    public static readonly Currency GBP = new("GBP", "£", 2);

    public static Currency FromCode(string code) => code switch
    {
        "EUR" => EUR,
        "USD" => USD,
        "GBP" => GBP,
        _ => throw new ArgumentException($"Currency {code} not supported")
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
public class CurrencyValidator : AbstractValidator<Currency>
{
    public CurrencyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(CurrencyValidationMessages.CodeRequired);

        RuleFor(x => x.Code)
            .Length(3)
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(CurrencyValidationMessages.CodeLength);

        RuleFor(x => x.Code)
            .Must(code => code == code?.ToUpperInvariant())
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(CurrencyValidationMessages.CodeUppercase);

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage(CurrencyValidationMessages.SymbolRequired);

        RuleFor(x => x.Symbol)
            .MaximumLength(5)
            .When(x => !string.IsNullOrEmpty(x.Symbol))
            .WithMessage(CurrencyValidationMessages.SymbolMaxLength);

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 4)
            .WithMessage(CurrencyValidationMessages.DecimalPlacesRange);
    }
}
