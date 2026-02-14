namespace Subscriptions.Features.Shared.Domain.Enums;

/// <summary>
/// Defines the type of a subscription feature.
/// </summary>
/// <remarks>
/// Determines how the feature limit is interpreted and enforced.
/// </remarks>
public enum FeatureType
{
    /// <summary>
    /// Feature is either enabled or disabled, no numeric limit.
    /// </summary>
    Boolean = 1,

    /// <summary>
    /// Feature has a specific numeric limit.
    /// </summary>
    Limit = 2,

    /// <summary>
    /// Feature has no usage restrictions.
    /// </summary>
    Unlimited = 3
}
