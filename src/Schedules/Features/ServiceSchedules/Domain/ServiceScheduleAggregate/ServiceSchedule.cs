namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

/// <summary>
/// Represents a service schedule aggregate root.
/// </summary>
/// <remarks>
/// <para>This aggregate manages the scheduling configuration for restaurant services.</para>
/// <para>It coordinates reservation policies, service configurations, and availability rules.</para>
/// </remarks>
public partial class ServiceSchedule : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    /// <value>The unique identifier of the tenant that owns this schedule.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets or sets the name of the schedule.
    /// </summary>
    /// <value>The schedule name. Maximum 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the schedule.
    /// </summary>
    /// <value>The schedule description. Maximum 500 characters.</value>
    public string Description { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this schedule is active.
    /// </summary>
    /// <value><c>true</c> if the schedule is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets or sets the reservation policy.
    /// </summary>
    /// <value>The policy that defines reservation rules and constraints.</value>
    public ReservationPolicy Policy { get; protected set; } = ReservationPolicy.Default();

    /// <summary>
    /// The internal collection of services.
    /// </summary>
    protected HashSet<Service> _services = [];

    /// <summary>
    /// Gets the read-only collection of services.
    /// </summary>
    /// <value>A read-only collection of <see cref="Service"/> instances.</value>
    public IReadOnlyCollection<Service> Services => _services.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this schedule has any services configured.
    /// </summary>
    /// <value><c>true</c> if there are services; otherwise, <c>false</c>.</value>
    public bool HasServices => _services.Any();

    /// <summary>
    /// Gets the number of services in this schedule.
    /// </summary>
    /// <value>The count of configured services.</value>
    public int ServiceCount => _services.Count;

    /// <summary>
    /// Gets the available service types in this schedule.
    /// </summary>
    /// <value>A read-only collection of service types that are configured.</value>
    public IReadOnlyCollection<ServiceType> AvailableServiceTypes => _services.Select(s => s.Type).ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected ServiceSchedule() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public ServiceSchedule(Guid id) : base(id) { }

    /// <summary>
    /// Gets the configuration for a specific service on a given date.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <param name="date">The date to get configuration for.</param>
    /// <returns>The service day configuration if found; otherwise, <c>null</c>.</returns>
    /// <remarks>Special dates have priority over weekly schedule configurations.</remarks>
    public ServiceDayConfig? GetServiceConfigForDate(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null) return null;

        var specialDate = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate != null)
        {
            return ServiceDayConfig.FromSpecialDate(specialDate);
        }

        return service.WeeklySchedule.TryGetValue(date.DayOfWeek, out var config) ? config : null;
    }

    /// <summary>
    /// Determines whether a service is available on a specific date.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if the service is available; otherwise, <c>false</c>.</returns>
    public bool IsServiceAvailableOn(ServiceType type, DateOnly date)
    {
        var config = GetServiceConfigForDate(type, date);
        return config?.IsAvailable ?? false;
    }

    /// <summary>
    /// Gets the capacity for a specific service on a given date.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <param name="date">The date to get capacity for.</param>
    /// <returns>The capacity if the service exists; otherwise, <c>null</c>.</returns>
    /// <remarks>Returns the capacity override if specified in the configuration; otherwise, returns the service's maximum capacity.</remarks>
    public int? GetServiceCapacityFor(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null) return null;

        var config = GetServiceConfigForDate(type, date);
        return config?.CapacityOverride ?? service.MaxCapacity;
    }
}

public static class ServiceScheduleValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "Tenant id is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 100 characters";
    public const string DescriptionRequired = "Description is required";
    public const string DescriptionMaxLength = "Description cannot exceed 500 characters";
    public const string PolicyRequired = "Reservation policy is required";
}

/// <summary>
/// Provides validation rules for the <see cref="ServiceSchedule"/> aggregate root.
/// </summary>
public class ServiceScheduleValidator : AbstractValidator<ServiceSchedule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceScheduleValidator"/> class.
    /// </summary>
    public ServiceScheduleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ServiceScheduleValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(ServiceScheduleValidationMessages.TenantIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ServiceScheduleValidationMessages.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage(ServiceScheduleValidationMessages.NameMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(ServiceScheduleValidationMessages.DescriptionRequired);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(ServiceScheduleValidationMessages.DescriptionMaxLength);

        RuleFor(x => x.Policy)
            .NotNull()
            .WithMessage(ServiceScheduleValidationMessages.PolicyRequired);
    }
}
