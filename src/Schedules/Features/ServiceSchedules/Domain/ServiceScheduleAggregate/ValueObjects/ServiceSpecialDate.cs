namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

/// <summary>
/// Represents a special date configuration for a service.
/// </summary>
/// <remarks>
/// <para>This value object defines service availability and hours for specific dates.</para>
/// <para>Special dates override the regular weekly schedule for services.</para>
/// </remarks>
public partial record ServiceSpecialDate
{
    /// <summary>
    /// Gets the date for this special configuration.
    /// </summary>
    /// <value>The specific date this configuration applies to.</value>
    public DateOnly Date { get; }

    /// <summary>
    /// Gets a value indicating whether the service is available on this date.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    public bool IsAvailable { get; }

    /// <summary>
    /// Gets the start time of the service.
    /// </summary>
    /// <value>The time when the service starts. Required when <see cref="IsAvailable"/> is <c>true</c>.</value>
    public TimeOnly? StartTime { get; }

    /// <summary>
    /// Gets the end time of the service.
    /// </summary>
    /// <value>The time when the service ends. Must be after <see cref="StartTime"/> when <see cref="IsAvailable"/> is <c>true</c>.</value>
    public TimeOnly? EndTime { get; }

    /// <summary>
    /// Gets the capacity override for this date.
    /// </summary>
    /// <value>An optional capacity override. Must be greater than 0 if specified.</value>
    public int? CapacityOverride { get; }

    /// <summary>
    /// Gets the reason for this special date.
    /// </summary>
    /// <value>An optional description explaining why this date has special configuration. Maximum 200 characters.</value>
    public string? Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceSpecialDate"/> record.
    /// </summary>
    /// <param name="date">The date for this special configuration.</param>
    /// <param name="isAvailable">Whether the service is available.</param>
    /// <param name="startTime">The start time of the service.</param>
    /// <param name="endTime">The end time of the service.</param>
    /// <param name="capacityOverride">The capacity override for this date.</param>
    /// <param name="reason">The reason for this special date.</param>
    protected ServiceSpecialDate(
        DateOnly date,
        bool isAvailable,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int? capacityOverride,
        string? reason)
    {
        Date = date;
        IsAvailable = isAvailable;
        StartTime = startTime;
        EndTime = endTime;
        CapacityOverride = capacityOverride;
        Reason = reason;
    }
}

public static class ServiceSpecialDateValidationMessages
{
    public const string DateRequired = "Date is required";
    public const string StartTimeRequired = "Start time is required when service is available";
    public const string EndTimeRequired = "End time is required when service is available";
    public const string EndTimeMustBeAfterStartTime = "End time must be after start time";
    public const string StartTimeMustBeEmpty = "Start time must be empty when service is not available";
    public const string EndTimeMustBeEmpty = "End time must be empty when service is not available";
    public const string CapacityOverrideMustBeGreaterThanZero = "Capacity override must be greater than 0";
    public const string ReasonMaxLength = "Reason cannot exceed 200 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="ServiceSpecialDate"/> value object.
/// </summary>
public class ServiceSpecialDateValidator : AbstractValidator<ServiceSpecialDate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceSpecialDateValidator"/> class.
    /// </summary>
    public ServiceSpecialDateValidator()
    {
        // Date is required
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage(ServiceSpecialDateValidationMessages.DateRequired);

        // When available, start time is required
        RuleFor(x => x.StartTime)
            .NotNull()
            .When(x => x.IsAvailable)
            .WithMessage(ServiceSpecialDateValidationMessages.StartTimeRequired);

        // When available, end time is required
        RuleFor(x => x.EndTime)
            .NotNull()
            .When(x => x.IsAvailable)
            .WithMessage(ServiceSpecialDateValidationMessages.EndTimeRequired);

        // When available, end time must be after start time
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .When(x => x.IsAvailable && x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage(ServiceSpecialDateValidationMessages.EndTimeMustBeAfterStartTime);

        // When not available, start time must be null
        RuleFor(x => x.StartTime)
            .Null()
            .When(x => !x.IsAvailable)
            .WithMessage(ServiceSpecialDateValidationMessages.StartTimeMustBeEmpty);

        // When not available, end time must be null
        RuleFor(x => x.EndTime)
            .Null()
            .When(x => !x.IsAvailable)
            .WithMessage(ServiceSpecialDateValidationMessages.EndTimeMustBeEmpty);

        // Capacity override must be greater than 0 if specified
        RuleFor(x => x.CapacityOverride)
            .GreaterThan(0)
            .When(x => x.CapacityOverride.HasValue)
            .WithMessage(ServiceSpecialDateValidationMessages.CapacityOverrideMustBeGreaterThanZero);

        // Reason cannot exceed 200 characters
        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage(ServiceSpecialDateValidationMessages.ReasonMaxLength);
    }
}
