namespace Plan.Features.Plans.Domain.PlanAggregate.Enums;

/// <summary>
/// Specifies the billing period for a subscription plan.
/// </summary>
/// <remarks>
/// This enum is used in <see cref="Plan"/> to determine
/// how frequently the customer will be billed for their subscription.
/// </remarks>
public enum BillingPeriod
{
    /// <summary>
    /// Billed every month.
    /// </summary>
    /// <remarks>
    /// The subscription renews monthly and the customer is charged every 30 days.
    /// </remarks>
    Monthly = 1,

    /// <summary>
    /// Billed every three months (quarterly).
    /// </summary>
    /// <remarks>
    /// The subscription renews every quarter and the customer is charged every 90 days.
    /// </remarks>
    Quarterly = 2,

    /// <summary>
    /// Billed every six months (semi-annually).
    /// </summary>
    /// <remarks>
    /// The subscription renews every semester and the customer is charged every 180 days.
    /// </remarks>
    Semester = 3,

    /// <summary>
    /// Billed every year (annually).
    /// </summary>
    /// <remarks>
    /// The subscription renews annually and the customer is charged every 365 days.
    /// </remarks>
    Yearly = 4
}
