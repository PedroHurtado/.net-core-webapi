namespace Schedules.UnitTests.Helpers;

public record TestableService(ServiceType type, int maxCapacity) : Service(type, maxCapacity)
{
    public Dictionary<DayOfWeek, ServiceDayConfig> WeeklyScheduleInternal
    {
        get => _weeklySchedule;
        set => _weeklySchedule = value;
    }

    public HashSet<ServiceSpecialDate> SpecialDatesInternal
    {
        get => _specialDates;
        set => _specialDates = value;
    }
}
