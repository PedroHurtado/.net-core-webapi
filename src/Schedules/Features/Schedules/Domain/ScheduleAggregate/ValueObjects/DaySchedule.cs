namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

/// <summary>
/// Represents a schedule for a specific day of the week.
/// </summary>
/// <remarks>
/// <para>This value object defines the operating hours for a single day.</para>
/// <para>A day can be closed or have one or more non-overlapping time slots.</para>
/// </remarks>
public partial record DaySchedule
{
    /// <summary>
    /// Gets the day of the week.
    /// </summary>
    /// <value>The day of the week this schedule applies to.</value>
    public DayOfWeek DayOfWeek { get; }

    /// <summary>
    /// Gets a value indicating whether the day is closed.
    /// </summary>
    /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
    public bool IsClosed { get; }

    /// <summary>
    /// Gets the time slots for this day.
    /// </summary>
    /// <value>A read-only collection of time slots. Empty when <see cref="IsClosed"/> is <c>true</c>.</value>
    public IReadOnlyCollection<TimeSlot> TimeSlots { get; }

    /// <summary>
    /// Gets the total open hours for this day.
    /// </summary>
    /// <value>The sum of all time slot durations.</value>
    public TimeSpan TotalOpenHours => TimeSlots.Aggregate(TimeSpan.Zero, (sum, ts) => sum + ts.Duration);

    /// <summary>
    /// Initializes a new instance of the <see cref="DaySchedule"/> record.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="isClosed">Whether the day is closed.</param>
    /// <param name="timeSlots">The time slots for this day.</param>
    protected DaySchedule(DayOfWeek dayOfWeek, bool isClosed, IReadOnlyCollection<TimeSlot> timeSlots)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        TimeSlots = timeSlots;
    }

    /// <summary>
    /// Determines whether the schedule is open at the specified time.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns><c>true</c> if open at the specified time; otherwise, <c>false</c>.</returns>
    public bool IsOpenAt(TimeOnly time) => !IsClosed && TimeSlots.Any(ts => ts.Contains(time));
}

public static class DayScheduleValidationMessages
{
    public const string InvalidDayOfWeek = "Invalid day of week";
    public const string ClosedDayCannotHaveTimeSlots = "Closed day cannot have time slots";
    public const string OpenDayMustHaveTimeSlots = "Open day must have at least one time slot";
    public const string TimeSlotsCannotOverlap = "Time slots cannot overlap";
}

/// <summary>
/// Provides validation rules for the <see cref="DaySchedule"/> value object.
/// </summary>
public class DayScheduleValidator : AbstractValidator<DaySchedule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DayScheduleValidator"/> class.
    /// </summary>
    public DayScheduleValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage(DayScheduleValidationMessages.InvalidDayOfWeek);

        RuleFor(x => x.TimeSlots)
            .Empty()
            .When(x => x.IsClosed)
            .WithMessage(DayScheduleValidationMessages.ClosedDayCannotHaveTimeSlots);

        RuleFor(x => x.TimeSlots)
            .NotEmpty()
            .When(x => !x.IsClosed)
            .WithMessage(DayScheduleValidationMessages.OpenDayMustHaveTimeSlots);

        RuleFor(x => x.TimeSlots)
            .Must(NotHaveOverlappingSlots)
            .When(x => x.TimeSlots.Count > 1)
            .WithMessage(DayScheduleValidationMessages.TimeSlotsCannotOverlap);
    }

    private static bool NotHaveOverlappingSlots(IReadOnlyCollection<TimeSlot> timeSlots)
    {
        var slotsList = timeSlots.ToList();
        for (var i = 0; i < slotsList.Count; i++)
        {
            for (var j = i + 1; j < slotsList.Count; j++)
            {
                if (slotsList[i].OverlapsWith(slotsList[j]))
                    return false;
            }
        }
        return true;
    }
}
