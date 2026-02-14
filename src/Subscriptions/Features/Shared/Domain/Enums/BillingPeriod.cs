namespace Subscriptions.Features.Shared.Domain.Enums;

/// <summary>
/// Represents the billing frequency for a subscription.
/// </summary>
/// <remarks>
/// Determines how often the customer is charged for the subscription.
/// </remarks>
public enum BillingPeriod
{
    /// <summary>
    /// Billed every month.
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Billed every three months.
    /// </summary>
    Quarterly = 2,

    /// <summary>
    /// Billed every six months.
    /// </summary>
    Semester = 3,

    /// <summary>
    /// Billed once a year.
    /// </summary>
    Yearly = 4
}