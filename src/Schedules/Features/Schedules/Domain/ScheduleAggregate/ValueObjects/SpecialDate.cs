namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

/// <summary>
/// Represents a special date with custom operating hours or closure.
/// </summary>
/// <remarks>
/// <para>This value object defines exceptions to the regular weekly schedule.</para>
/// <para>Used for holidays, special events, or any date requiring different hours.</para>
/// </remarks>
public partial record SpecialDate
{
    /// <summary>
    /// Gets the date.
    /// </summary>
    /// <value>The specific date this special schedule applies to.</value>
    public DateOnly Date { get; }

    /// <summary>
    /// Gets a value indicating whether the date is closed.
    /// </summary>
    /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
    public bool IsClosed { get; }

    /// <summary>
    /// Gets the reason for the special schedule.
    /// </summary>
    /// <value>A description of why this date has special hours. Maximum 200 characters.</value>
    public string Reason { get; }

    /// <summary>
    /// Gets the time slots for this date.
    /// </summary>
    /// <value>A read-only collection of time slots. Empty when <see cref="IsClosed"/> is <c>true</c>.</value>
    public IReadOnlyCollection<TimeSlot> TimeSlots { get; }

    /// <summary>
    /// Gets the total open hours for this date.
    /// </summary>
    /// <value>The sum of all time slot durations, or zero if closed.</value>
    public TimeSpan TotalOpenHours => IsClosed ? TimeSpan.Zero : TimeSlots.Aggregate(TimeSpan.Zero, (sum, ts) => sum + ts.Duration);

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDate"/> record.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="isClosed">Whether the date is closed.</param>
    /// <param name="reason">The reason for the special schedule.</param>
    /// <param name="timeSlots">The time slots for this date.</param>
    protected SpecialDate(DateOnly date, bool isClosed, string reason, IReadOnlyCollection<TimeSlot> timeSlots)
    {
        Date = date;
        IsClosed = isClosed;
        Reason = reason;
        TimeSlots = timeSlots;
    }

    /// <summary>
    /// Determines whether the schedule is open at the specified time.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns><c>true</c> if open at the specified time; otherwise, <c>false</c>.</returns>
    public bool IsOpenAt(TimeOnly time) => !IsClosed && TimeSlots.Any(ts => ts.Contains(time));
}

public static class SpecialDateValidationMessages
{
    public const string DateRequired = "Date is required";
    public const string ReasonRequired = "Reason is required";
    public const string ReasonMaxLength = "Reason cannot exceed 200 characters";
    public const string ClosedDateCannotHaveTimeSlots = "Closed date cannot have time slots";
    public const string OpenDateMustHaveTimeSlots = "Open date must have at least one time slot";
    public const string TimeSlotsCannotOverlap = "Time slots cannot overlap";
}

/// <summary>
/// Provides validation rules for the <see cref="SpecialDate"/> value object.
/// </summary>
public class SpecialDateValidator : AbstractValidator<SpecialDate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDateValidator"/> class.
    /// </summary>
    public SpecialDateValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage(SpecialDateValidationMessages.DateRequired);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(SpecialDateValidationMessages.ReasonRequired);

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage(SpecialDateValidationMessages.ReasonMaxLength);

        RuleFor(x => x.TimeSlots)
            .Empty()
            .When(x => x.IsClosed)
            .WithMessage(SpecialDateValidationMessages.ClosedDateCannotHaveTimeSlots);

        RuleFor(x => x.TimeSlots)
            .NotEmpty()
            .When(x => !x.IsClosed)
            .WithMessage(SpecialDateValidationMessages.OpenDateMustHaveTimeSlots);

        RuleFor(x => x.TimeSlots)
            .Must(NotHaveOverlappingSlots)
            .When(x => x.TimeSlots.Count > 1)
            .WithMessage(SpecialDateValidationMessages.TimeSlotsCannotOverlap);
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
