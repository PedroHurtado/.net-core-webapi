namespace Schedules.Features.Schedules.Api.ScheduleAggregate;

public record ScheduleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool HasWeeklyHours,
    bool HasSpecialDates,
    bool IsFullyConfigured,
    IReadOnlyDictionary<DayOfWeek, DayScheduleResponse> WeeklyHours,
    IReadOnlyCollection<SpecialDateResponse> SpecialDates)
{
    public static ScheduleResponse Map(Schedule schedule) => new(
        Id: schedule.Id,
        Name: schedule.Name,
        Description: schedule.Description,
        IsActive: schedule.IsActive,
        HasWeeklyHours: schedule.HasWeeklyHours,
        HasSpecialDates: schedule.HasSpecialDates,
        IsFullyConfigured: schedule.IsFullyConfigured,
        WeeklyHours: schedule.WeeklyHours
            .ToDictionary(
                kvp => kvp.Key,
                kvp => DayScheduleResponse.Map(kvp.Value))
            .AsReadOnly(),
        SpecialDates: schedule.SpecialDates
            .Select(SpecialDateResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record DayScheduleResponse(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    TimeSpan TotalOpenHours,
    IReadOnlyCollection<TimeSlotResponse> TimeSlots)
{
    public static DayScheduleResponse Map(DaySchedule daySchedule) => new(
        DayOfWeek: daySchedule.DayOfWeek,
        IsClosed: daySchedule.IsClosed,
        TotalOpenHours: daySchedule.TotalOpenHours,
        TimeSlots: daySchedule.TimeSlots
            .Select(TimeSlotResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record SpecialDateResponse(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    TimeSpan TotalOpenHours,
    IReadOnlyCollection<TimeSlotResponse> TimeSlots)
{
    public static SpecialDateResponse Map(SpecialDate specialDate) => new(
        Date: specialDate.Date,
        IsClosed: specialDate.IsClosed,
        Reason: specialDate.Reason,
        TotalOpenHours: specialDate.TotalOpenHours,
        TimeSlots: specialDate.TimeSlots
            .Select(TimeSlotResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record TimeSlotResponse(
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    TimeSpan Duration)
{
    public static TimeSlotResponse Map(TimeSlot timeSlot) => new(
        OpenTime: timeSlot.OpenTime,
        CloseTime: timeSlot.CloseTime,
        Duration: timeSlot.Duration);
}
