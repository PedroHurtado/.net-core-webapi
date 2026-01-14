using Fudie.Domain;
using Fudie;
using FluentValidation;

namespace Schedule.Features.ServiceSchedule.Models;

public class ServiceSchedule : Entity<Guid>
{
    public Guid RestaurantId { get; protected set; }
    public ReservationPolicy Policy { get; protected set; }

    public IReadOnlyCollection<Service> Services => _services.ToList().AsReadOnly();
    protected List<Service> _services = [];

    protected ServiceSchedule(Guid id, Guid restaurantId, ReservationPolicy policy) : base(id)
    {
        RestaurantId = restaurantId;
        Policy = policy;
    }

    public static Result<ServiceSchedule> Create(Guid id, Guid restaurantId, ReservationPolicy policy)
    {
        var serviceSchedule = new ServiceSchedule(id, restaurantId, policy);
        var validationResult = serviceSchedule.ValidateEntity(serviceSchedule, new ServiceScheduleValidator());

        if (validationResult.IsFailure)
        {
            return Result<ServiceSchedule>.Failure(validationResult.Errors);
        }

        return Result<ServiceSchedule>.Success(serviceSchedule);
    }

    public Result AddService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        if (_services.Any(s => s.Type == type))
        {
            return Result.Failure($"Ya existe un servicio de tipo {type}", "ServiceType");
        }

        if (maxCapacity <= 0)
        {
            return Result.Failure("Capacidad debe ser mayor que 0", "MaxCapacity");
        }

        // Validate time ranges in weekly schedule
        foreach (var (day, config) in weeklySchedule)
        {
            if (config.IsAvailable && config.StartTime >= config.EndTime)
            {
                return Result.Failure($"Hora de inicio debe ser antes de hora de fin para {day}", "WeeklySchedule");
            }

            if (config.CapacityOverride.HasValue && config.CapacityOverride.Value <= 0)
            {
                return Result.Failure($"Capacidad debe ser mayor que 0 para {day}", "CapacityOverride");
            }
        }

        var service = new Service(type, weeklySchedule, maxCapacity);
        _services.Add(service);
        
        return Result.Success();
    }

    public Result UpdateService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        if (maxCapacity <= 0)
        {
            return Result.Failure("Capacidad debe ser mayor que 0", "MaxCapacity");
        }

        // Validate time ranges
        foreach (var (day, config) in weeklySchedule)
        {
            if (config.IsAvailable && config.StartTime >= config.EndTime)
            {
                return Result.Failure($"Hora de inicio debe ser antes de hora de fin para {day}", "WeeklySchedule");
            }
        }

        _services.Remove(service);
        var updatedService = new Service(type, weeklySchedule, maxCapacity, service.SpecialDates.ToList());
        _services.Add(updatedService);

        return Result.Success();
    }

    public Result RemoveService(ServiceType type)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        _services.Remove(service);
        return Result.Success();
    }

    public Result ConfigureServiceDay(ServiceType type, DayOfWeek day, ServiceDayConfig config)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        if (config.IsAvailable && config.StartTime >= config.EndTime)
        {
            return Result.Failure("Hora de inicio debe ser antes de hora de fin", "ServiceDayConfig");
        }

        var weeklySchedule = service.WeeklySchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        weeklySchedule[day] = config;

        return UpdateService(type, weeklySchedule, service.MaxCapacity);
    }

    public Result AddSpecialDate(ServiceType type, DateOnly date, bool isAvailable, TimeOnly? startTime, TimeOnly? endTime, int? capacityOverride, string reason)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        if (service.SpecialDates.Any(sd => sd.Date == date))
        {
            return Result.Failure("Ya existe horario especial para esta fecha", "Date");
        }

        if (isAvailable)
        {
            if (!startTime.HasValue || !endTime.HasValue)
            {
                return Result.Failure("StartTime y EndTime son requeridos cuando IsAvailable=true", "SpecialDate");
            }
            // Note: We allow startTime >= endTime to support services that cross midnight (e.g., 19:00 to 02:00)
        }

        if (capacityOverride.HasValue && capacityOverride.Value <= 0)
        {
            return Result.Failure("Capacidad debe ser mayor que 0", "CapacityOverride");
        }

        var specialDate = new ServiceSpecialDate(date, isAvailable, startTime ?? TimeOnly.MinValue, endTime ?? TimeOnly.MinValue, capacityOverride, reason);
        var updatedSpecialDates = service.SpecialDates.Append(specialDate).ToList();
        
        _services.Remove(service);
        var weeklySchedule = service.WeeklySchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var updatedService = new Service(service.Type, weeklySchedule, service.MaxCapacity, updatedSpecialDates);
        _services.Add(updatedService);

        return Result.Success();
    }

    public Result RemoveSpecialDate(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        var specialDate = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate == null)
        {
            return Result.Failure("No existe horario especial para esta fecha", "Date");
        }

        var updatedSpecialDates = service.SpecialDates.Where(sd => sd.Date != date).ToList();
        
        _services.Remove(service);
        var weeklySchedule = service.WeeklySchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var updatedService = new Service(service.Type, weeklySchedule, service.MaxCapacity, updatedSpecialDates);
        _services.Add(updatedService);

        return Result.Success();
    }

    public Result UpdatePolicy(ReservationPolicy newPolicy)
    {
        var tempSchedule = new ServiceSchedule(Id, RestaurantId, newPolicy);
        var validationResult = tempSchedule.ValidateEntity(tempSchedule, new ServiceScheduleValidator());

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Policy = newPolicy;
        return Result.Success();
    }

    public List<ServiceType> GetAvailableServices(DateOnly date)
    {
        var availableServices = new List<ServiceType>();

        foreach (var service in _services)
        {
            var specialDate = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
            
            if (specialDate != null)
            {
                if (specialDate.IsAvailable)
                {
                    availableServices.Add(service.Type);
                }
            }
            else
            {
                var dayOfWeek = date.DayOfWeek;
                if (service.WeeklySchedule.TryGetValue(dayOfWeek, out var dayConfig) && dayConfig.IsAvailable)
                {
                    availableServices.Add(service.Type);
                }
            }
        }

        return availableServices;
    }

    public bool IsServiceAvailable(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return false;
        }

        var specialDate = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate != null)
        {
            return specialDate.IsAvailable;
        }

        var dayOfWeek = date.DayOfWeek;
        return service.WeeklySchedule.TryGetValue(dayOfWeek, out var dayConfig) && dayConfig.IsAvailable;
    }

    public Result<bool> CanReserve(ServiceType type, DateTime requestedDateTime, int partySize)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return Result<bool>.Failure($"No existe un servicio de tipo {type}", "ServiceType");
        }

        // Validate party size
        if (partySize < Policy.MinPartySize)
        {
            return Result<bool>.Failure($"Mínimo {Policy.MinPartySize} personas por reserva", "PartySize");
        }

        if (partySize > Policy.MaxPartySize)
        {
            return Result<bool>.Failure($"Máximo {Policy.MaxPartySize} personas por reserva. Contacte para grupos grandes.", "PartySize");
        }

        // Validate advance time
        var now = DateTime.Now;
        var timeDifference = requestedDateTime - now;

        if (timeDifference < Policy.MinimumAdvanceTime)
        {
            return Result<bool>.Failure($"Debe reservar con al menos {Policy.MinimumAdvanceTime.TotalHours} horas de antelación", "AdvanceTime");
        }

        if (timeDifference > Policy.MaximumAdvanceTime)
        {
            return Result<bool>.Failure($"Solo se aceptan reservas hasta {Policy.MaximumAdvanceTime.TotalDays} días de anticipación", "AdvanceTime");
        }

        // Validate slot interval
        var minutes = requestedDateTime.Minute;
        var slotMinutes = (int)Policy.SlotInterval.TotalMinutes;
        if (minutes % slotMinutes != 0)
        {
            return Result<bool>.Failure($"Solo se aceptan reservas cada {slotMinutes} minutos", "SlotInterval");
        }

        // Check if service is available on this date
        var date = DateOnly.FromDateTime(requestedDateTime);
        if (!IsServiceAvailable(type, date))
        {
            return Result<bool>.Failure("Servicio no disponible en esta fecha", "ServiceAvailability");
        }

        // Validate time is within service hours
        var config = GetServiceConfig(type, date);
        var requestedTime = TimeOnly.FromDateTime(requestedDateTime);
        
        bool crossesMidnight = config.EndTime <= config.StartTime;
        bool isWithinServiceHours;
        
        if (crossesMidnight)
        {
            // Service crosses midnight: valid if time >= startTime OR time < endTime
            isWithinServiceHours = requestedTime >= config.StartTime || requestedTime < config.EndTime;
        }
        else
        {
            // Normal service: valid if startTime <= time < endTime
            isWithinServiceHours = requestedTime >= config.StartTime && requestedTime < config.EndTime;
        }
        
        if (!isWithinServiceHours)
        {
            return Result<bool>.Failure($"Horario fuera del servicio ({config.StartTime} - {config.EndTime})", "ServiceHours");
        }

        // Validate sufficient time for service duration
        var duration = Policy.StandardDurations[type];
        var endTime = requestedTime.Add(duration);
        
        if (crossesMidnight)
        {
            // For midnight-crossing services, check if reservation fits
            // Either it ends before midnight, or it ends before the service end time (after midnight)
            bool fitsBeforeMidnight = requestedTime >= config.StartTime && endTime <= new TimeOnly(23, 59, 59);
            bool fitsAfterMidnight = requestedTime < config.EndTime && endTime <= config.EndTime;
            
            if (!fitsBeforeMidnight && !fitsAfterMidnight)
            {
                return Result<bool>.Failure("No hay tiempo suficiente para completar el servicio", "ServiceDuration");
            }
        }
        else
        {
            // Normal case
            if (endTime > config.EndTime)
            {
                return Result<bool>.Failure("No hay tiempo suficiente para completar el servicio", "ServiceDuration");
            }
        }

        return Result<bool>.Success(true);
    }

    public List<TimeOnly> GetAvailableSlots(ServiceType type, DateOnly date, int partySize)
    {
        var slots = new List<TimeOnly>();
        
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null || !IsServiceAvailable(type, date))
        {
            return slots;
        }

        var config = GetServiceConfig(type, date);
        var duration = Policy.StandardDurations[type];
        var slotInterval = Policy.SlotInterval;

        // Check if service crosses midnight
        bool crossesMidnight = config.EndTime <= config.StartTime;

        var currentTime = config.StartTime;
        
        if (crossesMidnight)
        {
            // For services crossing midnight, generate slots from startTime until end of day
            var maxTime = new TimeOnly(23, 59, 59);
            while (currentTime <= maxTime)
            {
                var endTime = currentTime.Add(duration);
                // Check if reservation fits before midnight
                if (endTime <= maxTime)
                {
                    slots.Add(currentTime);
                }
                
                var nextTime = currentTime.Add(slotInterval);
                if (nextTime <= currentTime) break; // Prevent infinite loop
                currentTime = nextTime;
            }
            
            // Add slots from midnight to service end time
            currentTime = TimeOnly.MinValue;
            while (currentTime < config.EndTime)
            {
                var endTime = currentTime.Add(duration);
                if (endTime <= config.EndTime)
                {
                    slots.Add(currentTime);
                }
                
                var nextTime = currentTime.Add(slotInterval);
                if (nextTime <= currentTime) break; // Prevent infinite loop
                currentTime = nextTime;
            }
        }
        else
        {
            // Normal case: service doesn't cross midnight
            while (currentTime < config.EndTime)
            {
                var endTime = currentTime.Add(duration);
                if (endTime <= config.EndTime)
                {
                    slots.Add(currentTime);
                }
                
                var nextTime = currentTime.Add(slotInterval);
                if (nextTime <= currentTime) break; // Prevent infinite loop
                currentTime = nextTime;
            }
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
        {
            return new ServiceDayConfig(false, TimeOnly.MinValue, TimeOnly.MinValue, null);
        }

        var specialDate = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
        if (specialDate != null)
        {
            return new ServiceDayConfig(specialDate.IsAvailable, specialDate.StartTime, specialDate.EndTime, specialDate.CapacityOverride);
        }

        var dayOfWeek = date.DayOfWeek;
        if (service.WeeklySchedule.TryGetValue(dayOfWeek, out var dayConfig))
        {
            return dayConfig;
        }

        return new ServiceDayConfig(false, TimeOnly.MinValue, TimeOnly.MinValue, null);
    }

    public int GetCapacity(ServiceType type, DateOnly date)
    {
        var service = _services.FirstOrDefault(s => s.Type == type);
        if (service == null)
        {
            return 0;
        }

        var config = GetServiceConfig(type, date);
        return config.CapacityOverride ?? service.MaxCapacity;
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
                .WithMessage("Policy es requerida");

            RuleFor(x => x.Policy.MinimumAdvanceTime)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Tiempo mínimo de antelación debe ser mayor que 0");

            RuleFor(x => x.Policy.MaximumAdvanceTime)
                .GreaterThan(x => x.Policy.MinimumAdvanceTime)
                .WithMessage("Tiempo máximo debe ser mayor que tiempo mínimo");

            RuleFor(x => x.Policy.SlotInterval)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Intervalo de slots debe ser mayor que 0");

            RuleFor(x => x.Policy.MaxPartySize)
                .GreaterThanOrEqualTo(x => x.Policy.MinPartySize)
                .WithMessage("Tamaño máximo debe ser mayor o igual que tamaño mínimo");

            RuleFor(x => x.Policy.MinPartySize)
                .GreaterThan(0)
                .WithMessage("Tamaño mínimo debe ser mayor que 0");
        }
    }
}

// Value Objects
public enum ServiceType
{
    Breakfast,
    Lunch,
    Dinner
}

public record Service
{
    public ServiceType Type { get; init; }
    public IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> WeeklySchedule { get; init; }
    public int MaxCapacity { get; init; }
    public IReadOnlyCollection<ServiceSpecialDate> SpecialDates { get; init; }

    public Service(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity, IEnumerable<ServiceSpecialDate>? specialDates = null)
    {
        Type = type;
        WeeklySchedule = weeklySchedule;
        MaxCapacity = maxCapacity;
        SpecialDates = (specialDates ?? Enumerable.Empty<ServiceSpecialDate>()).ToList().AsReadOnly();
    }
}

public record ServiceDayConfig(bool IsAvailable, TimeOnly StartTime, TimeOnly EndTime, int? CapacityOverride);

public record ServiceSpecialDate(DateOnly Date, bool IsAvailable, TimeOnly StartTime, TimeOnly EndTime, int? CapacityOverride, string Reason);

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

    public static ReservationPolicy CreateDefault()
    {
        return new ReservationPolicy(
            minimumAdvanceTime: TimeSpan.FromHours(2),
            maximumAdvanceTime: TimeSpan.FromDays(30),
            slotInterval: TimeSpan.FromMinutes(15),
            standardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Breakfast, TimeSpan.FromHours(1) },
                { ServiceType.Lunch, TimeSpan.FromHours(1.5) },
                { ServiceType.Dinner, TimeSpan.FromHours(2) }
            },
            bufferBetweenReservations: TimeSpan.FromMinutes(15),
            maxPartySize: 8,
            minPartySize: 1
        );
    }
}
