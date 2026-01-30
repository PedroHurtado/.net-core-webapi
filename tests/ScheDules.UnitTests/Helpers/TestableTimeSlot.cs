namespace Schedules.UnitTests.Helpers;

public record TestableTimeSlot(
    TimeOnly openTime,
    TimeOnly closeTime
) : TimeSlot(openTime, closeTime);
