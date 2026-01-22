using FluentValidation;

namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Represents a monetary value with its currency.
/// </summary>
/// <remarks>
/// <para>
/// This value object combines an amount with a currency to represent money.
/// It ensures that monetary values are always associated with their currency.
/// </para>
/// <para>
/// The value object provides methods for arithmetic operations that validate
/// currency compatibility before performing calculations.
/// </para>
/// </remarks>
public partial record Money
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    /// <value>The amount value. Must be greater than or equal to zero.</value>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency of this monetary value.
    /// </summary>
    /// <value>The currency associated with this amount.</value>
    public Currency Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> record.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency.</param>
    protected Money(
        decimal amount,
        Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a zero money value for the specified currency.
    /// </summary>
    /// <param name="currency">The currency for the zero amount.</param>
    /// <returns>A money instance with zero amount in the specified currency.</returns>
    public static Money Zero(Currency currency) => new(0, currency);

    /// <summary>
    /// Adds another money value to this instance.
    /// </summary>
    /// <param name="other">The money value to add.</param>
    /// <returns>A new money instance with the sum of both amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    /// <remarks>
    /// Both money values must have the same currency. Attempting to add
    /// money with different currencies will throw an exception.
    /// </remarks>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another money value from this instance.
    /// </summary>
    /// <param name="other">The money value to subtract.</param>
    /// <returns>A new money instance with the difference of both amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    /// <remarks>
    /// Both money values must have the same currency. Attempting to subtract
    /// money with different currencies will throw an exception.
    /// </remarks>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the amount by a factor.
    /// </summary>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>A new money instance with the multiplied amount.</returns>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Gets a value indicating whether this money value is zero.
    /// </summary>
    /// <value><c>true</c> if the amount is zero; otherwise, <c>false</c>.</value>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Gets a value indicating whether this money value is positive.
    /// </summary>
    /// <value><c>true</c> if the amount is greater than zero; otherwise, <c>false</c>.</value>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Gets a value indicating whether this money value is negative.
    /// </summary>
    /// <value><c>true</c> if the amount is less than zero; otherwise, <c>false</c>.</value>
    public bool IsNegative => Amount < 0;
}

public static class MoneyValidationMessages
{
    public const string AmountCannotBeNegative = "Amount cannot be negative";
    public const string CurrencyRequired = "Currency is required";
}

/// <summary>
/// Provides validation rules for the <see cref="Money"/> value object.
/// </summary>
/// <remarks>
/// Validates money invariants including non-negative amounts and required currency.
/// </remarks>
public class MoneyValidator : AbstractValidator<Money>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyValidator"/> class
    /// and configures all validation rules.
    /// </summary>
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
