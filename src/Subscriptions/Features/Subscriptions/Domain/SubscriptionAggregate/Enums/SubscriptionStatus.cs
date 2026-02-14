namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate.Enums;

/// <summary>
/// Represents the current lifecycle status of a subscription.
/// </summary>
/// <remarks>
/// Determines what actions are available and whether the subscription is usable by the tenant.
/// </remarks>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is in a free trial period.
    /// </summary>
    Trial = 1,

    /// <summary>
    /// Subscription is active and fully operational.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Payment has failed but the subscription is still usable during the grace period.
    /// </summary>
    PastDue = 3,

    /// <summary>
    /// Subscription has been cancelled by the owner, remains usable until the period ends.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Subscription has expired and is no longer usable.
    /// </summary>
    Expired = 5
}
