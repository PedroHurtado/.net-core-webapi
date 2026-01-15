namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Represents a deposit policy for menu reservations at the menu level.
/// </summary>
/// <remarks>
/// <para>
/// This value object defines how deposits are calculated for reservations.
/// The calculation method is determined by the <see cref="DepositType"/> property.
/// </para>
/// <para>
/// Optional thresholds can be configured to only require deposits when
/// the guest count or estimated bill exceeds certain minimums.
/// </para>
/// </remarks>
public partial record DepositPolicy
{
    /// <summary>
    /// Gets the type of deposit calculation to use.
    /// </summary>
    /// <value>The deposit calculation method.</value>
    public DepositType DepositType { get; }

    /// <summary>
    /// Gets the base amount used for deposit calculations.
    /// </summary>
    /// <value>
    /// For <see cref="Enums.DepositType.PerPerson"/>: the amount per guest.
    /// For <see cref="Enums.DepositType.FixedAmount"/>: the fixed deposit amount.
    /// Must be greater than zero.
    /// </value>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the percentage used for percentage-based deposit calculations.
    /// </summary>
    /// <value>
    /// A value between 1 and 100 when <see cref="DepositType"/> is
    /// <see cref="Enums.DepositType.PercentageOfBill"/>; otherwise, <c>null</c>.
    /// </value>
    public decimal? Percentage { get; }

    /// <summary>
    /// Gets the minimum estimated bill amount required to trigger the deposit.
    /// </summary>
    /// <value>The minimum bill threshold, or <c>null</c> if no threshold applies.</value>
    public decimal? MinimumBillForDeposit { get; }

    /// <summary>
    /// Gets the minimum number of guests required to trigger the deposit.
    /// </summary>
    /// <value>The minimum guest count threshold, or <c>null</c> if no threshold applies.</value>
    public int? MinimumGuestsForDeposit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DepositPolicy"/> record.
    /// </summary>
    /// <param name="depositType">The type of deposit calculation.</param>
    /// <param name="amount">The base amount for calculations.</param>
    /// <param name="percentage">The percentage for percentage-based calculations.</param>
    /// <param name="minimumBillForDeposit">The minimum bill threshold.</param>
    /// <param name="minimumGuestsForDeposit">The minimum guest count threshold.</param>
    protected DepositPolicy(
        DepositType depositType,
        decimal amount,
        decimal? percentage,
        decimal? minimumBillForDeposit,
        int? minimumGuestsForDeposit)
    {
        DepositType = depositType;
        Amount = amount;
        Percentage = percentage;
        MinimumBillForDeposit = minimumBillForDeposit;
        MinimumGuestsForDeposit = minimumGuestsForDeposit;
    }

    /// <summary>
    /// Determines whether the deposit policy applies based on the configured thresholds.
    /// </summary>
    /// <param name="guestCount">The number of guests in the reservation.</param>
    /// <param name="estimatedBill">The estimated bill total.</param>
    /// <returns>
    /// <c>true</c> if the deposit should be applied; <c>false</c> if the thresholds are not met.
    /// </returns>
    public bool IsApplicable(int guestCount, decimal estimatedBill)
    {
        if (MinimumGuestsForDeposit.HasValue && guestCount < MinimumGuestsForDeposit.Value)
            return false;

        if (MinimumBillForDeposit.HasValue && estimatedBill < MinimumBillForDeposit.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates the deposit amount based on the configured deposit type.
    /// </summary>
    /// <param name="guestCount">The number of guests in the reservation.</param>
    /// <param name="estimatedBill">The estimated bill total.</param>
    /// <returns>The calculated deposit amount.</returns>
    /// <remarks>
    /// The calculation varies by deposit type:
    /// <list type="bullet">
    /// <item><description><see cref="Enums.DepositType.PerPerson"/>: Amount × guestCount</description></item>
    /// <item><description><see cref="Enums.DepositType.PercentageOfBill"/>: estimatedBill × (Percentage / 100)</description></item>
    /// <item><description><see cref="Enums.DepositType.FixedAmount"/>: Amount (fixed)</description></item>
    /// </list>
    /// </remarks>
    public decimal CalculateDeposit(int guestCount, decimal estimatedBill)
    {
        return DepositType switch
        {
            DepositType.PerPerson => Amount * guestCount,
            DepositType.PercentageOfBill => estimatedBill * (Percentage!.Value / 100m),
            DepositType.FixedAmount => Amount,
            _ => 0m
        };
    }
}

public static class DepositPolicyValidationMessages
{
    public const string AmountGreaterThanZero = "Amount must be greater than zero";
    public const string PercentageRequiredForPercentageOfBill = "Percentage must be specified for PercentageOfBill type";
    public const string PercentageOnlyForPercentageOfBill = "Percentage only applies to PercentageOfBill type";
    public const string PercentageRange = "Percentage must be between 1 and 100";
    public const string MinimumGuestsForDepositMin = "MinimumGuestsForDeposit must be at least 1";
    public const string MinimumBillForDepositCannotBeNegative = "MinimumBillForDeposit cannot be negative";
}

/// <summary>
/// Provides validation rules for the <see cref="DepositPolicy"/> value object.
/// </summary>
/// <remarks>
/// Validates deposit policy invariants including amount constraints,
/// percentage requirements for percentage-based deposits, and threshold values.
/// </remarks>
public class DepositPolicyValidator : AbstractValidator<DepositPolicy>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepositPolicyValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public DepositPolicyValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(DepositPolicyValidationMessages.AmountGreaterThanZero);

        RuleFor(x => x.Percentage)
            .NotNull()
            .When(x => x.DepositType == DepositType.PercentageOfBill)
            .WithMessage(DepositPolicyValidationMessages.PercentageRequiredForPercentageOfBill);

        RuleFor(x => x.Percentage)
            .Null()
            .When(x => x.DepositType != DepositType.PercentageOfBill)
            .WithMessage(DepositPolicyValidationMessages.PercentageOnlyForPercentageOfBill);

        RuleFor(x => x.Percentage)
            .InclusiveBetween(1, 100)
            .When(x => x.Percentage.HasValue)
            .WithMessage(DepositPolicyValidationMessages.PercentageRange);

        RuleFor(x => x.MinimumGuestsForDeposit)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinimumGuestsForDeposit.HasValue)
            .WithMessage(DepositPolicyValidationMessages.MinimumGuestsForDepositMin);

        RuleFor(x => x.MinimumBillForDeposit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumBillForDeposit.HasValue)
            .WithMessage(DepositPolicyValidationMessages.MinimumBillForDepositCannotBeNegative);
    }
}
