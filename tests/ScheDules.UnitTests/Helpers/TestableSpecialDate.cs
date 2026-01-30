namespace Schedules.UnitTests.Helpers;

public record TestableSpecialDate(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    IReadOnlyCollection<TimeSlot> TimeSlots
) : SpecialDate(Date, IsClosed, Reason, TimeSlots);
