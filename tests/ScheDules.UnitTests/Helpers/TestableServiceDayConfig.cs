namespace Schedules.UnitTests.Helpers;

public record TestableServiceDayConfig(
    bool isAvailable,
    TimeOnly? startTime,
    TimeOnly? endTime,
    int? capacityOverride = null
) : ServiceDayConfig(isAvailable, startTime, endTime, capacityOverride);
