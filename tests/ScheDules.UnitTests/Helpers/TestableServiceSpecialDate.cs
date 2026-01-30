namespace Schedules.UnitTests.Helpers;

public record TestableServiceSpecialDate(
    DateOnly date,
    bool isAvailable,
    TimeOnly? startTime,
    TimeOnly? endTime,
    int? capacityOverride = null,
    string? reason = null
) : ServiceSpecialDate(date, isAvailable, startTime, endTime, capacityOverride, reason);
