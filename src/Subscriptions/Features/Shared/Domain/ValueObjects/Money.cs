namespace Subscriptions.Features.Shared.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with its associated currency.
/// </summary>
/// <remarks>
/// <para>Used as a ComplexType to store price information in aggregates.</para>
/// <para>Provides computed properties for zero/positive/negative checks and arithmetic operations.</para>
/// </remarks>
public partial record Money(
    decimal Amount,
    Currency Currency
)
{
    /// <summary>
    /// Gets a value indicating whether the amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Gets a value indicating whether the amount is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Gets a value indicating whether the amount is negative.
    /// </summary>
    public bool IsNegative => Amount < 0;

    public Money Add(Money other) => new(Amount + other.Amount, Currency);
    public Money Subtract(Money other) => new(Amount - other.Amount, Currency);
    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
}

public static class MoneyValidationMessages
{
    public const string AmountCannotBeNegative = "Amount cannot be negative";
    public const string CurrencyRequired = "Currency is required";
}

/// <summary>
/// Provides validation rules for the <see cref="Money"/> value object.
/// </summary>
public class MoneyValidator : AbstractValidator<Money>
{
    public MoneyValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MoneyValidationMessages.AmountCannotBeNegative);

        RuleFor(x => x.Currency)
            .NotNull()
            .WithMessage(MoneyValidationMessages.CurrencyRequired);
    }
}
