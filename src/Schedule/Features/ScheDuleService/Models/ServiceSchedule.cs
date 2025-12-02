using Fudie.Domain;
using Fudie;
using FluentValidation;

namespace Schedule.Features.ScheDuleService.Models;

#region Enums

public enum ServiceType
{
    Breakfast,
    Lunch,
    Dinner
}

#endregion

#region Value Objects

public record ServiceDayConfig
{
    public bool IsAvailable { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public int? CapacityOverride { get; init; }

    public ServiceDayConfig(bool isAvailable, TimeOnly startTime, TimeOnly endTime, int? capacityOverride = null)
    {
        IsAvailable = isAvailable;
        StartTime = startTime;
        EndTime = endTime;
        CapacityOverride = capacityOverride;
    }

    public static Result<ServiceDayConfig> Create(bool isAvailable, TimeOnly startTime, TimeOnly endTime, int? capacityOverride = null)
    {
        if (isAvailable)
        {
            if (startTime >= endTime)
                return Result<ServiceDayConfig>.Failure("Hora de inicio debe ser antes de hora de fin", "TimeRange");

            if (capacityOverride.HasValue && capacityOverride.Value <= 0)
                return Result<ServiceDayConfig>.Failure("Capacidad debe ser mayor que 0", "CapacityOverride");
        }
        else
        {
            if (startTime != default || endTime != default)
                return Result<ServiceDayConfig>.Failure("Si el servicio no está disponible, StartTime y EndTime deben ser default", "TimeRange");
        }

        return Result<ServiceDayConfig>.Success(new ServiceDayConfig(isAvailable, startTime, endTime, capacityOverride));
    }
}

public record ServiceSpecialDate
{
    public DateOnly Date { get; init; }
    public bool IsAvailable { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public int? CapacityOverride { get; init; }
    public string Reason { get; init; }

    public ServiceSpecialDate(DateOnly date, bool isAvailable, TimeOnly startTime, TimeOnly endTime, int? capacityOverride, string reason)
    {
        Date = date;
        IsAvailable = isAvailable;
        StartTime = startTime;
        EndTime = endTime;
        CapacityOverride = capacityOverride;
        Reason = reason;
    }

    public static Result<ServiceSpecialDate> Create(DateOnly date, bool isAvailable, TimeOnly startTime, TimeOnly endTime, int? capacityOverride, string reason)
    {
        if (date == default)
            return Result<ServiceSpecialDate>.Failure("La fecha es requerida", "Date");

        if (isAvailable && startTime >= endTime)
            return Result<ServiceSpecialDate>.Failure("Hora de inicio debe ser antes de hora de fin", "TimeRange");

        if (capacityOverride.HasValue && capacityOverride.Value <= 0)
            return Result<ServiceSpecialDate>.Failure("Capacidad debe ser mayor que 0", "CapacityOverride");

        if (!string.IsNullOrEmpty(reason) && reason.Length > 200)
            return Result<ServiceSpecialDate>.Failure("Razón no puede exceder 200 caracteres", "Reason");

        return Result<ServiceSpecialDate>.Success(new ServiceSpecialDate(date, isAvailable, startTime, endTime, capacityOverride, reason));
    }
}

public class Service
{
    public ServiceType Type { get; protected set; }
    public int MaxCapacity { get; protected set; }

    public IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> WeeklySchedule => _weeklySchedule;
    protected Dictionary<DayOfWeek, ServiceDayConfig> _weeklySchedule = new();

    public IReadOnlyCollection<ServiceSpecialDate> SpecialDates => _specialDates.ToList().AsReadOnly();
    protected List<ServiceSpecialDate> _specialDates = new();

    protected Service(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        Type = type;
        MaxCapacity = maxCapacity;
        _weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>(weeklySchedule);
    }

    public static Result<Service> Create(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        if (maxCapacity <= 0)
            return Result<Service>.Failure("Capacidad debe ser mayor que 0", "MaxCapacity");

        if (weeklySchedule == null || weeklySchedule.Count == 0)
            return Result<Service>.Failure("Debe configurar al menos un día de la semana", "WeeklySchedule");

        return Result<Service>.Success(new Service(type, weeklySchedule, maxCapacity));
    }

    public Result AddSpecialDate(ServiceSpecialDate specialDate)
    {
        if (specialDate == null)
            return Result.Failure("Fecha especial no puede ser nula", "SpecialDate");

        if (_specialDates.Any(sd => sd.Date == specialDate.Date))
            return Result.Failure("Ya existe horario especial para esta fecha", "SpecialDate");

        _specialDates.Add(specialDate);
        return Result.Success();
    }

    public Result RemoveSpecialDate(DateOnly date)
    {
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate == null)
            return Result.Failure("No existe horario especial para esta fecha", "SpecialDate");

        _specialDates.Remove(specialDate);
        return Result.Success();
    }

    public Result UpdateWeeklySchedule(Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule)
    {
        if (weeklySchedule == null || weeklySchedule.Count == 0)
            return Result.Failure("Debe configurar al menos un día de la semana", "WeeklySchedule");

        _weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>(weeklySchedule);
        return Result.Success();
    }

    public Result UpdateMaxCapacity(int maxCapacity)
    {
        if (maxCapacity <= 0)
            return Result.Failure("Capacidad debe ser mayor que 0", "MaxCapacity");

        MaxCapacity = maxCapacity;
        return Result.Success();
    }

    public ServiceDayConfig GetConfigForDate(DateOnly date)
    {
        // First check for special dates
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate != null)
        {
            return new ServiceDayConfig(
                specialDate.IsAvailable,
                specialDate.StartTime,
                specialDate.EndTime,
                specialDate.CapacityOverride
            );
        }

        // Fall back to weekly schedule
        var dayOfWeek = date.DayOfWeek;
        if (_weeklySchedule.TryGetValue(dayOfWeek, out var config))
            return config;

        return new ServiceDayConfig(false, default, default, null);
    }

    public int GetCapacityForDate(DateOnly date)
    {
        var config = GetConfigForDate(date);
        return config.CapacityOverride ?? MaxCapacity;
    }
}

public record ReservationPolicy
{
    public TimeSpan MinimumAdvanceTime { get; init; }
    public TimeSpan MaximumAdvanceTime { get; init; }
    public TimeSpan SlotInterval { get; init; }
    public Dictionary<ServiceType, TimeSpan> StandardDurations { get; init; }
    public TimeSpan BufferBetweenReservations { get; init; }
    public int MaxPartySize { get; init; }
    public int MinPartySize { get; init; }

    public ReservationPolicy(
        TimeSpan minimumAdvanceTime,
        TimeSpan maximumAdvanceTime,
        TimeSpan slotInterval,
        Dictionary<ServiceType, TimeSpan> standardDurations,
        TimeSpan bufferBetweenReservations,
        int maxPartySize,
        int minPartySize)
    {
        MinimumAdvanceTime = minimumAdvanceTime;
        MaximumAdvanceTime = maximumAdvanceTime;
        SlotInterval = slotInterval;
        StandardDurations = standardDurations;
        BufferBetweenReservations = bufferBetweenReservations;
        MaxPartySize = maxPartySize;
        MinPartySize = minPartySize;
    }

    public static Result<ReservationPolicy> Create(
        TimeSpan minimumAdvanceTime,
        TimeSpan maximumAdvanceTime,
        TimeSpan slotInterval,
        Dictionary<ServiceType, TimeSpan> standardDurations,
        TimeSpan bufferBetweenReservations,
        int maxPartySize,
        int minPartySize)
    {
        if (minimumAdvanceTime <= TimeSpan.Zero)
            return Result<ReservationPolicy>.Failure("Tiempo mínimo de antelación debe ser mayor que 0", "MinimumAdvanceTime");

        if (maximumAdvanceTime <= minimumAdvanceTime)
            return Result<ReservationPolicy>.Failure("Tiempo máximo debe ser mayor que tiempo mínimo", "MaximumAdvanceTime");

        if (slotInterval <= TimeSpan.Zero)
            return Result<ReservationPolicy>.Failure("Intervalo de slots debe ser mayor que 0", "SlotInterval");

        if (standardDurations == null || standardDurations.Count == 0)
            return Result<ReservationPolicy>.Failure("Debe configurar duraciones estándar para los servicios", "StandardDurations");

        if (bufferBetweenReservations < TimeSpan.Zero)
            return Result<ReservationPolicy>.Failure("Buffer no puede ser negativo", "BufferBetweenReservations");

        if (maxPartySize <= 0)
            return Result<ReservationPolicy>.Failure("Tamaño máximo de grupo debe ser mayor que 0", "MaxPartySize");

        if (minPartySize <= 0)
            return Result<ReservationPolicy>.Failure("Tamaño mínimo de grupo debe ser mayor que 0", "MinPartySize");

        if (minPartySize > maxPartySize)
            return Result<ReservationPolicy>.Failure("Tamaño mínimo no puede ser mayor que tamaño máximo", "PartySize");

        return Result<ReservationPolicy>.Success(new ReservationPolicy(
            minimumAdvanceTime,
            maximumAdvanceTime,
            slotInterval,
            standardDurations,
            bufferBetweenReservations,
            maxPartySize,
            minPartySize
        ));
    }
}

#endregion

#region ServiceSchedule Entity

public class ServiceSchedule : Entity
{
    public Guid RestaurantId { get; protected set; }
    public ReservationPolicy Policy { get; protected set; }

    public IReadOnlyCollection<Service> Services => _services.ToList().AsReadOnly();
    protected List<Service> _services = new();

    protected ServiceSchedule(Guid id, Guid restaurantId, ReservationPolicy policy) : base(id)
    {
        RestaurantId = restaurantId;
        Policy = policy;
    }

    public static Result<ServiceSchedule> Create(Guid id, Guid restaurantId, ReservationPolicy policy)
    {
        var schedule = new ServiceSchedule(id, restaurantId, policy);
        var validationResult = ValidateEntity(schedule, new ServiceScheduleValidator());

        if (validationResult.IsFailure)
            return Result<ServiceSchedule>.Failure(validationResult.Errors);

        return Result<ServiceSchedule>.Success(schedule);
    }

    public Result AddService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        if (_services.Any(s => s.Type == type))
            return Result.Failure($"Ya existe un servicio de tipo {type}", "ServiceType");

        var serviceResult = Service.Create(type, weeklySchedule, maxCapacity);
        if (serviceResult.IsFailure)
            return Result.Failure(serviceResult.Errors);

        _services.Add(serviceResult.Value);
        return Result.Success();
    }

    public Result UpdateService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        var updateScheduleResult = service.UpdateWeeklySchedule(weeklySchedule);
        if (updateScheduleResult.IsFailure)
            return updateScheduleResult;

        var updateCapacityResult = service.UpdateMaxCapacity(maxCapacity);
        if (updateCapacityResult.IsFailure)
            return updateCapacityResult;

        return Result.Success();
    }

    public Result RemoveService(ServiceType type)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        _services.Remove(service);
        return Result.Success();
    }

    public Result ConfigureServiceDay(ServiceType type, DayOfWeek day, ServiceDayConfig config)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        var newSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>(service.WeeklySchedule)
        {
            [day] = config
        };

        return service.UpdateWeeklySchedule(newSchedule);
    }

    public Result AddSpecialDate(ServiceType type, DateOnly date, bool isAvailable, TimeOnly startTime, TimeOnly endTime, int? capacityOverride, string reason)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        var specialDateResult = ServiceSpecialDate.Create(date, isAvailable, startTime, endTime, capacityOverride, reason);
        if (specialDateResult.IsFailure)
            return Result.Failure(specialDateResult.Errors);

        return service.AddSpecialDate(specialDateResult.Value);
    }

    public Result RemoveSpecialDate(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        return service.RemoveSpecialDate(date);
    }

    public Result UpdatePolicy(ReservationPolicy newPolicy)
    {
        if (newPolicy == null)
            return Result.Failure("La política no puede ser nula", "Policy");

        Policy = newPolicy;
        return Result.Success();
    }

    public List<ServiceType> GetAvailableServices(DateOnly date)
    {
        return _services
            .Where(s => s.GetConfigForDate(date).IsAvailable)
            .Select(s => s.Type)
            .ToList();
    }

    public bool IsServiceAvailable(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return false;

        var config = service.GetConfigForDate(date);
        return config.IsAvailable;
    }

    public Result CanReserve(ServiceType type, DateTime requestedDateTime, int partySize)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return Result.Failure($"No existe servicio de tipo {type}", "ServiceType");

        var date = DateOnly.FromDateTime(requestedDateTime);
        var time = TimeOnly.FromDateTime(requestedDateTime);
        var config = service.GetConfigForDate(date);

        if (!config.IsAvailable)
            return Result.Failure($"Servicio {type} no disponible en esta fecha", "Availability");

        var now = DateTime.Now;
        var timeDifference = requestedDateTime - now;

        if (timeDifference < Policy.MinimumAdvanceTime)
            return Result.Failure($"Debe reservar con al menos {Policy.MinimumAdvanceTime.TotalHours} horas de antelación", "MinimumAdvanceTime");

        if (timeDifference > Policy.MaximumAdvanceTime)
            return Result.Failure($"Solo se aceptan reservas hasta {Policy.MaximumAdvanceTime.TotalDays} días de anticipación", "MaximumAdvanceTime");

        if (partySize < Policy.MinPartySize)
            return Result.Failure($"Mínimo {Policy.MinPartySize} personas por reserva", "PartySize");

        if (partySize > Policy.MaxPartySize)
            return Result.Failure($"Máximo {Policy.MaxPartySize} personas por reserva", "PartySize");

        if (time < config.StartTime || time > config.EndTime)
            return Result.Failure($"Horario fuera del rango de servicio ({config.StartTime}-{config.EndTime})", "TimeRange");

        var slotIntervalMinutes = (int)Policy.SlotInterval.TotalMinutes;
        if (time.Minute % slotIntervalMinutes != 0 || time.Second != 0)
            return Result.Failure($"Solo se aceptan reservas cada {slotIntervalMinutes} minutos", "SlotInterval");

        if (!Policy.StandardDurations.TryGetValue(type, out var duration))
            return Result.Failure("No hay duración estándar configurada para este servicio", "StandardDuration");

        var endTime = time.Add(duration);
        if (endTime > config.EndTime)
            return Result.Failure("No hay tiempo suficiente para completar el servicio", "Duration");

        return Result.Success();
    }

    public List<TimeOnly> GetAvailableSlots(ServiceType type, DateOnly date, int partySize)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return new List<TimeOnly>();

        var config = service.GetConfigForDate(date);
        if (!config.IsAvailable)
            return new List<TimeOnly>();

        if (!Policy.StandardDurations.TryGetValue(type, out var duration))
            return new List<TimeOnly>();

        var slots = new List<TimeOnly>();
        var currentSlot = config.StartTime;
        var lastValidSlot = config.EndTime.Add(-duration);

        while (currentSlot <= lastValidSlot)
        {
            var requestedDateTime = date.ToDateTime(currentSlot);
            var canReserveResult = CanReserve(type, requestedDateTime, partySize);

            if (canReserveResult.IsSuccess)
            {
                slots.Add(currentSlot);
            }

            currentSlot = currentSlot.Add(Policy.SlotInterval);
        }

        return slots;
    }

    public DateTime CalculateReservationEndTime(ServiceType type, DateTime startTime)
    {
        if (Policy.StandardDurations.TryGetValue(type, out var duration))
        {
            return startTime.Add(duration).Add(Policy.BufferBetweenReservations);
        }

        return startTime;
    }

    public ServiceDayConfig GetServiceConfig(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return new ServiceDayConfig(false, default, default, null);

        return service.GetConfigForDate(date);
    }

    public int GetCapacity(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
            return 0;

        return service.GetCapacityForDate(date);
    }

    protected class ServiceScheduleValidator : AbstractValidator<ServiceSchedule>
    {
        public ServiceScheduleValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty()
                .WithMessage("RestaurantId es requerido");

            RuleFor(x => x.Policy)
                .NotNull()
                .WithMessage("La política de reservas es requerida");
        }
    }
}

#endregion
