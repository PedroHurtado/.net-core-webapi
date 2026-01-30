namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.Enums;

/// <summary>
/// Represents the type of meal service offered.
/// </summary>
/// <remarks>
/// Used to categorize services by meal period throughout the day.
/// </remarks>
public enum ServiceType
{
    /// <summary>
    /// Morning meal service.
    /// </summary>
    /// <remarks>
    /// Typically served in the early hours of the day.
    /// </remarks>
    Breakfast = 1,

    /// <summary>
    /// Midday meal service.
    /// </summary>
    /// <remarks>
    /// Typically served during the middle of the day.
    /// </remarks>
    Lunch = 2,

    /// <summary>
    /// Evening meal service.
    /// </summary>
    /// <remarks>
    /// Typically served in the evening hours.
    /// </remarks>
    Dinner = 3
}
