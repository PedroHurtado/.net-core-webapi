namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

/// <summary>
/// Represents reservation policy rules and constraints.
/// </summary>
/// <remarks>
/// <para>This value object defines the rules for making reservations.</para>
/// <para>Includes advance time windows, slot intervals, party size limits, and service durations.</para>
/// </remarks>
public partial record ReservationPolicy
{
    /// <summary>
    /// Gets the minimum advance time required for reservations.
    /// </summary>
    /// <value>The minimum time before a service that a reservation can be made. Must be greater than zero.</value>
    public TimeSpan MinimumAdvanceTime { get; }

    /// <summary>
    /// Gets the maximum advance time allowed for reservations.
    /// </summary>
    /// <value>The maximum time in advance that reservations can be made. Must be greater than minimum advance time.</value>
    public TimeSpan MaximumAdvanceTime { get; }

    /// <summary>
    /// Gets the time interval between available reservation slots.
    /// </summary>
    /// <value>The slot interval. Must be 15, 30, or 60 minutes.</value>
    public TimeSpan SlotInterval { get; }

    /// <summary>
    /// Gets the buffer time between consecutive reservations.
    /// </summary>
    /// <value>The buffer time to allow for table turnover. Cannot be negative.</value>
    public TimeSpan BufferBetweenReservations { get; }

    /// <summary>
    /// Gets the maximum party size allowed.
    /// </summary>
    /// <value>The maximum number of people per reservation. Must be greater than 0.</value>
    public int MaxPartySize { get; }

    /// <summary>
    /// Gets the minimum party size allowed.
    /// </summary>
    /// <value>The minimum number of people per reservation. Must be greater than 0 and not exceed max party size.</value>
    public int MinPartySize { get; }

    /// <summary>
    /// Gets the standard durations for each service type.
    /// </summary>
    /// <value>A read-only dictionary mapping service types to their standard durations.</value>
    protected Dictionary<ServiceType, TimeSpan> _standardDurations = [];
    public IReadOnlyDictionary<ServiceType, TimeSpan> StandardDurations => _standardDurations.AsReadOnly();

    /// <summary>
    /// Gets the slot interval in minutes.
    /// </summary>
    /// <value>The slot interval converted to minutes.</value>
    public int SlotIntervalMinutes => (int)SlotInterval.TotalMinutes;

    /// <summary>
    /// Gets the maximum advance booking window in days.
    /// </summary>
    /// <value>The maximum advance time converted to days.</value>
    public int MaxAdvanceDays => (int)MaximumAdvanceTime.TotalDays;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationPolicy"/> record.
    /// </summary>
    /// <param name="minimumAdvanceTime">The minimum advance time.</param>
    /// <param name="maximumAdvanceTime">The maximum advance time.</param>
    /// <param name="slotInterval">The slot interval.</param>
    /// <param name="bufferBetweenReservations">The buffer between reservations.</param>
    /// <param name="maxPartySize">The maximum party size.</param>
    /// <param name="minPartySize">The minimum party size.</param>
    protected ReservationPolicy(
        TimeSpan minimumAdvanceTime,
        TimeSpan maximumAdvanceTime,
        TimeSpan slotInterval,
        TimeSpan bufferBetweenReservations,
        int maxPartySize,
        int minPartySize)
    {
        MinimumAdvanceTime = minimumAdvanceTime;
        MaximumAdvanceTime = maximumAdvanceTime;
        SlotInterval = slotInterval;
        BufferBetweenReservations = bufferBetweenReservations;
        MaxPartySize = maxPartySize;
        MinPartySize = minPartySize;
    }

    /// <summary>
    /// Determines whether a given time is a valid reservation slot.
    /// </summary>
    /// <param name="time">The time to validate.</param>
    /// <returns><c>true</c> if the time is a multiple of the slot interval; otherwise, <c>false</c>.</returns>
    public bool IsValidSlot(TimeOnly time)
    {
        var totalMinutes = time.Hour * 60 + time.Minute;
        return totalMinutes % SlotIntervalMinutes == 0;
    }

    /// <summary>
    /// Gets the standard duration for a specific service type.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <returns>The standard duration for the service type, or <c>TimeSpan.Zero</c> if not configured.</returns>
    public TimeSpan GetDurationFor(ServiceType type)
    {
        return _standardDurations.TryGetValue(type, out var duration) ? duration : TimeSpan.Zero;
    }

    /// <summary>
    /// Determines whether a party size is valid.
    /// </summary>
    /// <param name="partySize">The party size to validate.</param>
    /// <returns><c>true</c> if the party size is within the allowed range; otherwise, <c>false</c>.</returns>
    public bool IsPartySizeValid(int partySize)
    {
        return partySize >= MinPartySize && partySize <= MaxPartySize;
    }

    /// <summary>
    /// Determines whether a requested time is within the advance booking window.
    /// </summary>
    /// <param name="requestedTime">The requested reservation time.</param>
    /// <param name="now">The current time.</param>
    /// <returns><c>true</c> if the requested time is within the advance window; otherwise, <c>false</c>.</returns>
    public bool IsWithinAdvanceWindow(DateTime requestedTime, DateTime now)
    {
        var timeDifference = requestedTime - now;
        return timeDifference >= MinimumAdvanceTime && timeDifference <= MaximumAdvanceTime;
    }

    /// <summary>
    /// Creates a default reservation policy.
    /// </summary>
    /// <returns>A reservation policy with standard default values.</returns>
    public static ReservationPolicy Default() => new(
        minimumAdvanceTime: TimeSpan.FromHours(2),
        maximumAdvanceTime: TimeSpan.FromDays(30),
        slotInterval: TimeSpan.FromMinutes(30),
        bufferBetweenReservations: TimeSpan.FromMinutes(15),
        maxPartySize: 8,
        minPartySize: 1);
}

public static class ReservationPolicyValidationMessages
{
    public const string MinimumAdvanceTimeMustBeGreaterThanZero = "Minimum advance time must be greater than 0";
    public const string MaximumAdvanceTimeMustBeGreaterThanMinimum = "Maximum advance time must be greater than minimum advance time";
    public const string SlotIntervalMustBeGreaterThanZero = "Slot interval must be greater than 0";
    public const string SlotIntervalMustBeValid = "Slot interval must be 15, 30, or 60 minutes";
    public const string BufferCannotBeNegative = "Buffer cannot be negative";
    public const string MaxPartySizeMustBeGreaterThanZero = "Max party size must be greater than 0";
    public const string MinPartySizeMustBeGreaterThanZero = "Min party size must be greater than 0";
    public const string MinPartySizeCannotExceedMax = "Min party size cannot exceed max party size";
}

/// <summary>
/// Provides validation rules for the <see cref="ReservationPolicy"/> value object.
/// </summary>
public class ReservationPolicyValidator : AbstractValidator<ReservationPolicy>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationPolicyValidator"/> class.
    /// </summary>
    public ReservationPolicyValidator()
    {
        // MinimumAdvanceTime must be greater than zero
        RuleFor(x => x.MinimumAdvanceTime)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage(ReservationPolicyValidationMessages.MinimumAdvanceTimeMustBeGreaterThanZero);

        // MaximumAdvanceTime must be greater than MinimumAdvanceTime
        RuleFor(x => x.MaximumAdvanceTime)
            .GreaterThan(x => x.MinimumAdvanceTime)
            .WithMessage(ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum);

        // SlotInterval must be greater than zero
        RuleFor(x => x.SlotInterval)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage(ReservationPolicyValidationMessages.SlotIntervalMustBeGreaterThanZero);

        // SlotInterval must be 15, 30, or 60 minutes
        RuleFor(x => x.SlotInterval)
            .Must(interval => interval == TimeSpan.FromMinutes(15) ||
                             interval == TimeSpan.FromMinutes(30) ||
                             interval == TimeSpan.FromMinutes(60))
            .WithMessage(ReservationPolicyValidationMessages.SlotIntervalMustBeValid);

        // BufferBetweenReservations cannot be negative
        RuleFor(x => x.BufferBetweenReservations)
            .GreaterThanOrEqualTo(TimeSpan.Zero)
            .WithMessage(ReservationPolicyValidationMessages.BufferCannotBeNegative);

        // MaxPartySize must be greater than 0
        RuleFor(x => x.MaxPartySize)
            .GreaterThan(0)
            .WithMessage(ReservationPolicyValidationMessages.MaxPartySizeMustBeGreaterThanZero);

        // MinPartySize must be greater than 0
        RuleFor(x => x.MinPartySize)
            .GreaterThan(0)
            .WithMessage(ReservationPolicyValidationMessages.MinPartySizeMustBeGreaterThanZero);

        // MinPartySize cannot exceed MaxPartySize
        RuleFor(x => x.MinPartySize)
            .LessThanOrEqualTo(x => x.MaxPartySize)
            .WithMessage(ReservationPolicyValidationMessages.MinPartySizeCannotExceedMax);
    }
}
