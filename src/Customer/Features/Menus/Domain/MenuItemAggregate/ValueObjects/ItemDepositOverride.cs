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
public partial record ItemDepositOverride
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
    protected ItemDepositOverride(
        decimal depositAmount,
        int? minimumQuantityForDeposit)
    {
        DepositAmount = depositAmount;
        MinimumQuantityForDeposit = minimumQuantityForDeposit;
    }

    /// <summary>
    /// Gets a value indicating whether the deposit applies to all quantities.
    /// </summary>
    /// <value><c>true</c> if no minimum quantity is specified; otherwise, <c>false</c>.</value>
    public bool AppliesToAllQuantities => !MinimumQuantityForDeposit.HasValue;

    /// <summary>
    /// Determines whether the deposit applies based on the ordered quantity.
    /// </summary>
    /// <param name="quantity">The quantity of items being ordered.</param>
    /// <returns>
    /// <c>true</c> if the deposit should be applied; <c>false</c> if the quantity threshold is not met.
    /// </returns>
    public bool IsApplicable(int quantity)
    {
        return AppliesToAllQuantities || quantity >= MinimumQuantityForDeposit!.Value;
    }
}

public static class ItemDepositOverrideValidationMessages
{
    public const string DepositAmountGreaterThanZero = "Deposit amount must be greater than zero";
    public const string DepositAmountMax = "Deposit amount cannot exceed 10000";
    public const string MinimumQuantityMin = "Minimum quantity must be at least 1";
    public const string MinimumQuantityMax = "Minimum quantity cannot exceed 100";
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
            .WithMessage(ItemDepositOverrideValidationMessages.DepositAmountGreaterThanZero)
            .LessThanOrEqualTo(10000)
            .WithMessage(ItemDepositOverrideValidationMessages.DepositAmountMax);

        RuleFor(x => x.MinimumQuantityForDeposit)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinimumQuantityForDeposit.HasValue)
            .WithMessage(ItemDepositOverrideValidationMessages.MinimumQuantityMin)
            .LessThanOrEqualTo(100)
            .When(x => x.MinimumQuantityForDeposit.HasValue)
            .WithMessage(ItemDepositOverrideValidationMessages.MinimumQuantityMax);
    }
}
