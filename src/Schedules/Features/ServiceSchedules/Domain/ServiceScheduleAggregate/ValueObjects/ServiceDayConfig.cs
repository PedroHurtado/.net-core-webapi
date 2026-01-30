namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

/// <summary>
/// Represents the configuration for a service on a specific day.
/// </summary>
/// <remarks>
/// <para>This value object defines whether a service is available on a day and its operating hours.</para>
/// <para>When unavailable, start and end times must be null. When available, both times are required.</para>
/// </remarks>
public partial record ServiceDayConfig
{
    /// <summary>
    /// Gets a value indicating whether the service is available on this day.
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
    /// Gets the capacity override for this day.
    /// </summary>
    /// <value>An optional capacity override. Must be greater than 0 if specified.</value>
    public int? CapacityOverride { get; }

    /// <summary>
    /// Gets the duration of the service.
    /// </summary>
    /// <value>The time span between <see cref="StartTime"/> and <see cref="EndTime"/>, or <c>null</c> if unavailable.</value>
    public TimeSpan? Duration => IsAvailable && StartTime.HasValue && EndTime.HasValue
        ? EndTime.Value - StartTime.Value
        : null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDayConfig"/> record.
    /// </summary>
    /// <param name="isAvailable">Whether the service is available.</param>
    /// <param name="startTime">The start time of the service.</param>
    /// <param name="endTime">The end time of the service.</param>
    /// <param name="capacityOverride">The capacity override for this day.</param>
    protected ServiceDayConfig(bool isAvailable, TimeOnly? startTime, TimeOnly? endTime, int? capacityOverride)
    {
        IsAvailable = isAvailable;
        StartTime = startTime;
        EndTime = endTime;
        CapacityOverride = capacityOverride;
    }

    /// <summary>
    /// Creates an unavailable service day configuration.
    /// </summary>
    /// <returns>A <see cref="ServiceDayConfig"/> with <see cref="IsAvailable"/> set to <c>false</c>.</returns>
    public static ServiceDayConfig Unavailable() => new(false, null, null, null);
}

public static class ServiceDayConfigValidationMessages
{
    public const string StartTimeRequired = "Start time is required when service is available";
    public const string EndTimeRequired = "End time is required when service is available";
    public const string EndTimeMustBeAfterStartTime = "End time must be after start time";
    public const string StartTimeMustBeEmpty = "Start time must be empty when service is not available";
    public const string EndTimeMustBeEmpty = "End time must be empty when service is not available";
    public const string CapacityOverrideMustBeGreaterThanZero = "Capacity override must be greater than 0";
}

/// <summary>
/// Provides validation rules for the <see cref="ServiceDayConfig"/> value object.
/// </summary>
public class ServiceDayConfigValidator : AbstractValidator<ServiceDayConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDayConfigValidator"/> class.
    /// </summary>
    public ServiceDayConfigValidator()
    {
        // When available, start time is required
        RuleFor(x => x.StartTime)
            .NotNull()
            .When(x => x.IsAvailable)
            .WithMessage(ServiceDayConfigValidationMessages.StartTimeRequired);

        // When available, end time is required
        RuleFor(x => x.EndTime)
            .NotNull()
            .When(x => x.IsAvailable)
            .WithMessage(ServiceDayConfigValidationMessages.EndTimeRequired);

        // When available, end time must be after start time
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .When(x => x.IsAvailable && x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage(ServiceDayConfigValidationMessages.EndTimeMustBeAfterStartTime);

        // When not available, start time must be null
        RuleFor(x => x.StartTime)
            .Null()
            .When(x => !x.IsAvailable)
            .WithMessage(ServiceDayConfigValidationMessages.StartTimeMustBeEmpty);

        // When not available, end time must be null
        RuleFor(x => x.EndTime)
            .Null()
            .When(x => !x.IsAvailable)
            .WithMessage(ServiceDayConfigValidationMessages.EndTimeMustBeEmpty);

        // Capacity override must be greater than 0 if specified
        RuleFor(x => x.CapacityOverride)
            .GreaterThan(0)
            .When(x => x.CapacityOverride.HasValue)
            .WithMessage(ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero);
    }
}
