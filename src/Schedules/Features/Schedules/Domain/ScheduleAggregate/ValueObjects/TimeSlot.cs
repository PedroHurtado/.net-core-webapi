namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

/// <summary>
/// Represents a time slot with open and close times.
/// </summary>
/// <remarks>
/// <para>This value object defines a time range for scheduling purposes.</para>
/// <para>Provides methods to check containment and overlap with other time slots.</para>
/// </remarks>
public partial record TimeSlot
{
    /// <summary>
    /// Gets the opening time of the slot.
    /// </summary>
    /// <value>The time when the slot opens.</value>
    public TimeOnly OpenTime { get; }

    /// <summary>
    /// Gets the closing time of the slot.
    /// </summary>
    /// <value>The time when the slot closes. Must be after <see cref="OpenTime"/>.</value>
    public TimeOnly CloseTime { get; }

    /// <summary>
    /// Gets the duration of the time slot.
    /// </summary>
    /// <value>The time span between <see cref="OpenTime"/> and <see cref="CloseTime"/>.</value>
    public TimeSpan Duration => CloseTime - OpenTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSlot"/> record.
    /// </summary>
    /// <param name="openTime">The opening time.</param>
    /// <param name="closeTime">The closing time.</param>
    protected TimeSlot(TimeOnly openTime, TimeOnly closeTime)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    /// <summary>
    /// Determines whether the specified time is within this time slot.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns><c>true</c> if the time is within the slot; otherwise, <c>false</c>.</returns>
    public bool Contains(TimeOnly time) => time >= OpenTime && time <= CloseTime;

    /// <summary>
    /// Determines whether this time slot overlaps with another time slot.
    /// </summary>
    /// <param name="other">The other time slot to check.</param>
    /// <returns><c>true</c> if the slots overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapsWith(TimeSlot other) => OpenTime < other.CloseTime && CloseTime > other.OpenTime;
}

public static class TimeSlotValidationMessages
{
    public const string OpenTimeRequired = "Open time is required";
    public const string CloseTimeRequired = "Close time is required";
    public const string CloseTimeMustBeAfterOpenTime = "Close time must be after open time";
}

/// <summary>
/// Provides validation rules for the <see cref="TimeSlot"/> value object.
/// </summary>
public class TimeSlotValidator : AbstractValidator<TimeSlot>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSlotValidator"/> class.
    /// </summary>
    public TimeSlotValidator()
    {
        RuleFor(x => x.CloseTime)
            .GreaterThan(x => x.OpenTime)
            .WithMessage(TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime);
    }
}