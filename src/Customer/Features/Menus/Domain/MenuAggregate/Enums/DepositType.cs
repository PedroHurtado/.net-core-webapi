namespace Customer.Features.Menus.Domain.MenuAggregate.Enums;

/// <summary>
/// Specifies the calculation method for reservation deposits.
/// </summary>
/// <remarks>
/// This enum is used in <see cref="ValueObjects.DepositPolicy"/> to determine
/// how the deposit amount should be calculated for a reservation.
/// </remarks>
public enum DepositType
{
    /// <summary>
    /// Deposit calculated as a fixed amount per guest.
    /// </summary>
    /// <remarks>
    /// The total deposit is calculated as: Amount × Number of Guests.
    /// </remarks>
    PerPerson = 1,

    /// <summary>
    /// Deposit calculated as a percentage of the estimated bill total.
    /// </summary>
    /// <remarks>
    /// When using this type, the <see cref="ValueObjects.DepositPolicy.Percentage"/>
    /// property must be specified.
    /// </remarks>
    PercentageOfBill = 2,

    /// <summary>
    /// A fixed deposit amount regardless of the number of guests.
    /// </summary>
    /// <remarks>
    /// The deposit amount is fixed and does not vary based on party size or estimated bill.
    /// </remarks>
    FixedAmount = 3
}
