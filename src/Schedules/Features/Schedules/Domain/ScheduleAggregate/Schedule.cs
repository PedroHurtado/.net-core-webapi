namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

/// <summary>
/// Represents a schedule configuration for a tenant.
/// </summary>
/// <remarks>
/// <para>This aggregate root follows the DDD pattern with protected setters and read-only collections.</para>
/// <para>Manages weekly hours and special dates for business operations scheduling.</para>
/// </remarks>
public partial class Schedule : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    /// <value>The unique identifier of the tenant that owns this schedule.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the schedule name.
    /// </summary>
    /// <value>The name of the schedule. Cannot exceed 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the schedule description.
    /// </summary>
    /// <value>An optional description of the schedule. Cannot exceed 500 characters.</value>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this schedule is active.
    /// </summary>
    /// <value><c>true</c> if the schedule is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// The internal collection of weekly hours.
    /// </summary>
    protected Dictionary<DayOfWeek, DaySchedule> _weeklyHours = [];

    /// <summary>
    /// Gets the read-only dictionary of weekly hours.
    /// </summary>
    /// <value>A read-only dictionary mapping <see cref="DayOfWeek"/> to <see cref="DaySchedule"/> instances.</value>
    public IReadOnlyDictionary<DayOfWeek, DaySchedule> WeeklyHours => _weeklyHours.AsReadOnly();

    /// <summary>
    /// The internal collection of special dates.
    /// </summary>
    protected HashSet<SpecialDate> _specialDates = [];

    /// <summary>
    /// Gets the read-only collection of special dates.
    /// </summary>
    /// <value>A read-only collection of <see cref="SpecialDate"/> instances.</value>
    public IReadOnlyCollection<SpecialDate> SpecialDates => _specialDates.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this schedule has weekly hours configured.
    /// </summary>
    /// <value><c>true</c> if weekly hours are configured; otherwise, <c>false</c>.</value>
    public bool HasWeeklyHours => _weeklyHours.Any();

    /// <summary>
    /// Gets a value indicating whether this schedule has special dates configured.
    /// </summary>
    /// <value><c>true</c> if special dates are configured; otherwise, <c>false</c>.</value>
    public bool HasSpecialDates => _specialDates.Any();

    /// <summary>
    /// Gets a value indicating whether this schedule is fully configured.
    /// </summary>
    /// <value><c>true</c> if all 7 days of the week are configured; otherwise, <c>false</c>.</value>
    public bool IsFullyConfigured => _weeklyHours.Count == 7;

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected Schedule() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public Schedule(Guid id) : base(id) { }
}

public static class ScheduleValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "Tenant id is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 100 characters";
    public const string DescriptionMaxLength = "Description cannot exceed 500 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="Schedule"/> aggregate root.
/// </summary>
public class ScheduleValidator : AbstractValidator<Schedule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleValidator"/> class.
    /// </summary>
    public ScheduleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ScheduleValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(ScheduleValidationMessages.TenantIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ScheduleValidationMessages.NameRequired)
            .MaximumLength(100)
            .WithMessage(ScheduleValidationMessages.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(ScheduleValidationMessages.DescriptionMaxLength);
    }
}
