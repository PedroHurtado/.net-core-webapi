namespace Plans.Features.Plans.Domain.PlanAggregate.Enums;

/// <summary>
/// Specifies the type of feature in a subscription plan.
/// </summary>
/// <remarks>
/// This enum is used in <see cref="ValueObjects.Feature"/> to determine
/// how the feature should be interpreted and validated.
/// </remarks>
public enum FeatureType
{
    /// <summary>
    /// Feature is either included or not (boolean flag).
    /// </summary>
    /// <remarks>
    /// Used for features that don't have a numeric limit, such as "Priority Support"
    /// or "Advanced Reports". When this type is used, the Limit property should be null.
    /// </remarks>
    Boolean = 1,

    /// <summary>
    /// Feature has a specific numeric limit.
    /// </summary>
    /// <remarks>
    /// Used for features with a maximum allowed value, such as "100 reservations per month"
    /// or "4 active waiters". When this type is used, the Limit property must have a value greater than 0.
    /// </remarks>
    Limit = 2,

    /// <summary>
    /// Feature has no limit (unlimited usage).
    /// </summary>
    /// <remarks>
    /// Used for features that have no restrictions, such as "Unlimited reservations".
    /// When this type is used, the Limit property should be null.
    /// </remarks>
    Unlimited = 3
}
