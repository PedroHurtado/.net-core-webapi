namespace Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

/// <summary>
/// Represents a deposit override for a specific menu item that supersedes the menu's default policy.
/// </summary>
/// <remarks>
/// <para>
/// This value object allows individual menu items to have their own deposit requirements,
/// which take precedence over the menu-level deposit policy.
/// </para>
/// <para>
/// Useful for high-value items or items requiring special preparation where
/// a specific deposit amount is needed regardless of the menu's general policy.
/// </para>
/// </remarks>
public record ItemDepositOverride
{
    /// <summary>
    /// Gets the fixed deposit amount for this item.
    /// </summary>
    /// <value>The deposit amount. Must be greater than zero.</value>
    public decimal DepositAmount { get; }

    /// <summary>
    /// Gets the minimum quantity of items that triggers the deposit requirement.
    /// </summary>
    /// <value>
    /// The minimum quantity threshold, or <c>null</c> if the deposit applies to any quantity.
    /// Must be at least 1 when specified.
    /// </value>
    public int? MinimumQuantityForDeposit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDepositOverride"/> record.
    /// </summary>
    /// <param name="depositAmount">The fixed deposit amount.</param>
    /// <param name="minimumQuantityForDeposit">The minimum quantity threshold.</param>
    private ItemDepositOverride(
        decimal depositAmount,
        int? minimumQuantityForDeposit)
    {
        DepositAmount = depositAmount;
        MinimumQuantityForDeposit = minimumQuantityForDeposit;
    }

    /// <summary>
    /// Creates a new validated <see cref="ItemDepositOverride"/> instance.
    /// </summary>
    /// <param name="depositAmount">The fixed deposit amount. Must be greater than zero.</param>
    /// <param name="minimumQuantityForDeposit">Optional minimum quantity threshold (must be at least 1 if specified).</param>
    /// <returns>A validated <see cref="ItemDepositOverride"/> instance.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static ItemDepositOverride Create(
        decimal depositAmount,
        int? minimumQuantityForDeposit = null)
    {
        var itemOverride = new ItemDepositOverride(depositAmount, minimumQuantityForDeposit);

        return new ItemDepositOverrideValidator().ValidateOrThrow(itemOverride);
    }

    /// <summary>
    /// Determines whether the deposit applies based on the ordered quantity.
    /// </summary>
    /// <param name="quantity">The quantity of items being ordered.</param>
    /// <returns>
    /// <c>true</c> if the deposit should be applied; <c>false</c> if the quantity threshold is not met.
    /// </returns>
    public bool IsApplicable(int quantity)
    {
        if (MinimumQuantityForDeposit.HasValue)
            return quantity >= MinimumQuantityForDeposit.Value;

        return true;
    }
}

public static class ItemDepositOverrideValidationMessages
{
    public const string GreaterThanZero = "{PropertyName} must be greater than zero";
    public const string MinValue = "{PropertyName} must be at least {ComparisonValue}";
}

/// <summary>
/// Provides validation rules for the <see cref="ItemDepositOverride"/> value object.
/// </summary>
/// <remarks>
/// Validates item deposit override invariants including deposit amount and quantity threshold constraints.
/// </remarks>
public class ItemDepositOverrideValidator : AbstractValidator<ItemDepositOverride>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDepositOverrideValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public ItemDepositOverrideValidator()
    {
        RuleFor(x => x.DepositAmount)
            .GreaterThan(0)
            .WithMessage(ItemDepositOverrideValidationMessages.GreaterThanZero);

        RuleFor(x => x.MinimumQuantityForDeposit)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinimumQuantityForDeposit.HasValue)
            .WithMessage(ItemDepositOverrideValidationMessages.MinValue);
    }
}
