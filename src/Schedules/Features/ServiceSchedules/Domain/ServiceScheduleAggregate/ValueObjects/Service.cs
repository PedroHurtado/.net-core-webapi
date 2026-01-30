namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

/// <summary>
/// Represents a service configuration with weekly schedule and special dates.
/// </summary>
/// <remarks>
/// <para>This value object defines a service type with its capacity and availability schedule.</para>
/// <para>Services can have different configurations for each day of the week and special date overrides.</para>
/// </remarks>
public partial record Service
{
    /// <summary>
    /// Gets the type of service.
    /// </summary>
    /// <value>The service type (Breakfast, Lunch, or Dinner).</value>
    public ServiceType Type { get; }

    /// <summary>
    /// Gets the maximum capacity for this service.
    /// </summary>
    /// <value>The maximum number of reservations allowed. Must be greater than 0.</value>
    public int MaxCapacity { get; }

    /// <summary>
    /// Gets the weekly schedule configuration.
    /// </summary>
    /// <value>A read-only dictionary mapping days of the week to their configurations.</value>
    protected Dictionary<DayOfWeek, ServiceDayConfig> _weeklySchedule = [];
    public IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> WeeklySchedule => _weeklySchedule.AsReadOnly();

    /// <summary>
    /// Gets the special dates configuration.
    /// </summary>
    /// <value>A read-only collection of special date configurations that override the weekly schedule.</value>
    protected HashSet<ServiceSpecialDate> _specialDates = [];
    public IReadOnlyCollection<ServiceSpecialDate> SpecialDates => _specialDates.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this service has special dates configured.
    /// </summary>
    /// <value><c>true</c> if there are special dates; otherwise, <c>false</c>.</value>
    public bool HasSpecialDates => _specialDates.Any();

    /// <summary>
    /// Gets the number of days per week this service is available.
    /// </summary>
    /// <value>The count of days where the service is available.</value>
    public int AvailableDaysCount => _weeklySchedule.Count(kvp => kvp.Value.IsAvailable);

    /// <summary>
    /// Initializes a new instance of the <see cref="Service"/> record.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <param name="maxCapacity">The maximum capacity.</param>
    protected Service(ServiceType type, int maxCapacity)
    {
        Type = type;
        MaxCapacity = maxCapacity;
    }

    /// <summary>
    /// Gets the configuration for a specific date.
    /// </summary>
    /// <param name="date">The date to get configuration for.</param>
    /// <returns>The special date configuration if it exists; otherwise, the weekly schedule configuration for that day of week.</returns>
    /// <remarks>Special dates take precedence over the weekly schedule.</remarks>
    public ServiceDayConfig? GetConfigForDate(DateOnly date)
    {
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate != null)
        {
            return new ServiceDayConfig(
                specialDate.IsAvailable,
                specialDate.StartTime,
                specialDate.EndTime,
                specialDate.CapacityOverride);
        }

        return _weeklySchedule.TryGetValue(date.DayOfWeek, out var config) ? config : null;
    }

    /// <summary>
    /// Determines whether the service is available on a specific date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if the service is available; otherwise, <c>false</c>.</returns>
    public bool IsAvailableOn(DateOnly date)
    {
        var config = GetConfigForDate(date);
        return config?.IsAvailable ?? false;
    }

    /// <summary>
    /// Gets the capacity for a specific date.
    /// </summary>
    /// <param name="date">The date to get capacity for.</param>
    /// <returns>The capacity override if specified in the configuration; otherwise, the maximum capacity.</returns>
    public int GetCapacityFor(DateOnly date)
    {
        var config = GetConfigForDate(date);
        return config?.CapacityOverride ?? MaxCapacity;
    }
}

public static class ServiceValidationMessages
{
    public const string InvalidServiceType = "Invalid service type";
    public const string MaxCapacityMustBeGreaterThanZero = "Max capacity must be greater than 0";
    public const string ServiceMustBeAvailableAtLeastOneDay = "Service must be available at least one day per week";
}

/// <summary>
/// Provides validation rules for the <see cref="Service"/> value object.
/// </summary>
public class ServiceValidator : AbstractValidator<Service>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceValidator"/> class.
    /// </summary>
    public ServiceValidator()
    {
        // Type must be a valid enum value
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(ServiceValidationMessages.InvalidServiceType);

        // MaxCapacity must be greater than 0
        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .WithMessage(ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero);

        // Service must be available at least one day per week
        RuleFor(x => x.WeeklySchedule)
            .Must(schedule => schedule.Any(kvp => kvp.Value.IsAvailable))
            .WithMessage(ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay);
    }
}
