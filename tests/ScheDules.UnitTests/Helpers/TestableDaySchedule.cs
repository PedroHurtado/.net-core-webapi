namespace Schedules.UnitTests.Helpers;

public record TestableDaySchedule(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    IReadOnlyCollection<TimeSlot> TimeSlots
) : DaySchedule(DayOfWeek, IsClosed, TimeSlots);
