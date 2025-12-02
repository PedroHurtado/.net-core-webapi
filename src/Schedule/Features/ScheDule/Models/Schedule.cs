using Fudie.Domain;
using Fudie;
using FluentValidation;

namespace Schedule.Features.Schedule.models;

public class Schedule : Entity
{
    public Guid RestaurantId { get; protected set; }

    public IReadOnlyDictionary<DayOfWeek, DaySchedule> WeeklyHours =>
        _weeklyHours.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    protected Dictionary<DayOfWeek, DaySchedule> _weeklyHours = new();

    public IReadOnlyCollection<SpecialDate> SpecialDates => _specialDates.AsReadOnly();

    protected List<SpecialDate> _specialDates = new();

    protected Schedule(Guid id, Guid restaurantId) : base(id)
    {
        RestaurantId = restaurantId;

        // Inicializar todos los días de la semana como cerrados por defecto
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            _weeklyHours[day] = new DaySchedule(day, true, new List<TimeSlot>());
        }
    }

    public static Result<Schedule> Create(Guid id, Guid restaurantId)
    {
        var schedule = new Schedule(id, restaurantId);
        var validationResult = ValidateEntity(schedule, new ScheduleValidator());

        if (validationResult.IsFailure)
        {
            return Result<Schedule>.Failure(validationResult.Errors);
        }

        return Result<Schedule>.Success(schedule);
    }

    public Result SetWeeklyHours(DayOfWeek day, List<TimeSlot> slots)
    {
        // Validar que los slots sean válidos
        var validation = ValidateTimeSlots(slots);
        if (validation.IsFailure)
        {
            return validation;
        }

        // Validar que no haya solapamiento
        var overlapValidation = ValidateNoOverlapping(slots);
        if (overlapValidation.IsFailure)
        {
            return overlapValidation;
        }

        _weeklyHours[day] = new DaySchedule(day, false, slots);
        return Result.Success();
    }

    public Result MarkDayAsClosed(DayOfWeek day)
    {
        _weeklyHours[day] = new DaySchedule(day, true, new List<TimeSlot>());
        return Result.Success();
    }

    public Result AddSpecialDate(DateOnly date, bool isClosed, string reason, List<TimeSlot>? slots = null)
    {
        // Validar que la fecha no esté duplicada
        if (_specialDates.Any(sd => sd.Date == date))
        {
            return Result.Failure("Ya existe un horario especial para esta fecha", nameof(date));
        }

        // Si está cerrado, no debe tener slots
        if (isClosed && slots != null && slots.Any())
        {
            return Result.Failure("Un día cerrado no puede tener horarios", nameof(slots));
        }

        // Si no está cerrado, debe tener slots
        if (!isClosed && (slots == null || slots.Count == 0))
        {
            return Result.Failure("Un día abierto debe tener horarios", nameof(slots));
        }

        // Validar los slots si existen
        if (slots != null && slots.Count != 0)
        {
            var validation = ValidateTimeSlots(slots);
            if (validation.IsFailure)
            {
                return validation;
            }

            var overlapValidation = ValidateNoOverlapping(slots);
            if (overlapValidation.IsFailure)
            {
                return overlapValidation;
            }
        }

        var specialDate = new SpecialDate(date, isClosed, reason, slots ?? new List<TimeSlot>());
        _specialDates.Add(specialDate);
        return Result.Success();
    }

    public Result RemoveSpecialDate(DateOnly date)
    {
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);

        if (specialDate == null)
        {
            return Result.Failure("No existe un horario especial para esta fecha", nameof(date));
        }

        _specialDates.Remove(specialDate);
        return Result.Success();
    }

    public Result UpdateSpecialDate(DateOnly date, bool isClosed, string reason, List<TimeSlot>? slots = null)
    {
        var removeResult = RemoveSpecialDate(date);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        return AddSpecialDate(date, isClosed, reason, slots);
    }

    public bool IsOpen(DateTime dateTime)
    {
        var date = DateOnly.FromDateTime(dateTime);
        var time = TimeOnly.FromDateTime(dateTime);

        // Buscar fecha especial
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);

        if (specialDate != null)
        {
            if (specialDate.IsClosed)
            {
                return false;
            }

            return specialDate.TimeSlots.Any(slot =>
                time >= slot.OpenTime && time <= slot.CloseTime);
        }

        // Usar horario semanal
        var daySchedule = _weeklyHours[dateTime.DayOfWeek];

        if (daySchedule.IsClosed)
        {
            return false;
        }

        return daySchedule.TimeSlots.Any(slot =>
            time >= slot.OpenTime && time <= slot.CloseTime);
    }

    public List<TimeSlot> GetHoursFor(DateOnly date)
    {
        // Buscar fecha especial
        var specialDate = _specialDates.FirstOrDefault(sd => sd.Date == date);

        if (specialDate != null)
        {
            return specialDate.IsClosed ? new List<TimeSlot>() : specialDate.TimeSlots.ToList();
        }

        // Usar horario semanal
        var dayOfWeek = date.DayOfWeek;
        var daySchedule = _weeklyHours[dayOfWeek];

        return daySchedule.IsClosed ? new List<TimeSlot>() : daySchedule.TimeSlots.ToList();
    }

    public bool IsSpecialDate(DateOnly date)
    {
        return _specialDates.Any(sd => sd.Date == date);
    }

    private static Result ValidateTimeSlots(List<TimeSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (slot.OpenTime >= slot.CloseTime)
            {
                return Result.Failure("Hora de apertura debe ser antes del cierre", "TimeSlot");
            }
        }

        return Result.Success();
    }

    private static Result ValidateNoOverlapping(List<TimeSlot> slots)
    {
        var sortedSlots = slots.OrderBy(s => s.OpenTime).ToList();

        for (int i = 0; i < sortedSlots.Count - 1; i++)
        {
            if (sortedSlots[i].CloseTime > sortedSlots[i + 1].OpenTime)
            {
                return Result.Failure("Los horarios no pueden solaparse", "TimeSlot");
            }
        }

        return Result.Success();
    }

    protected class ScheduleValidator : AbstractValidator<Schedule>
    {
        public ScheduleValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty()
                .WithMessage("RestaurantId es requerido");
        }
    }
}

// Value Objects

public record TimeSlot
{
    public TimeOnly OpenTime { get; init; }
    public TimeOnly CloseTime { get; init; }

    public TimeSlot(TimeOnly openTime, TimeOnly closeTime)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
    }
}

public record DaySchedule
{
    public DayOfWeek DayOfWeek { get; init; }
    public bool IsClosed { get; init; }
    public IReadOnlyCollection<TimeSlot> TimeSlots { get; init; }

    public DaySchedule(DayOfWeek dayOfWeek, bool isClosed, List<TimeSlot> timeSlots)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        TimeSlots = timeSlots.AsReadOnly();
    }
}

public record SpecialDate
{
    public DateOnly Date { get; init; }
    public bool IsClosed { get; init; }
    public string Reason { get; init; }
    public IReadOnlyCollection<TimeSlot> TimeSlots { get; init; }

    public SpecialDate(DateOnly date, bool isClosed, string reason, List<TimeSlot> timeSlots)
    {
        Date = date;
        IsClosed = isClosed;
        Reason = reason;
        TimeSlots = timeSlots.AsReadOnly();
    }
}
