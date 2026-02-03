namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate;

public record ServiceScheduleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    ReservationPolicyResponse Policy,
    bool HasServices,
    int ServiceCount,
    IReadOnlyCollection<ServiceType> AvailableServiceTypes,
    IReadOnlyCollection<ServiceResponse> Services)
{
    public static ServiceScheduleResponse Map(ServiceSchedule serviceSchedule) => new(
        Id: serviceSchedule.Id,
        Name: serviceSchedule.Name,
        Description: serviceSchedule.Description,
        IsActive: serviceSchedule.IsActive,
        Policy: ReservationPolicyResponse.Map(serviceSchedule.Policy),
        HasServices: serviceSchedule.HasServices,
        ServiceCount: serviceSchedule.ServiceCount,
        AvailableServiceTypes: serviceSchedule.AvailableServiceTypes
            .ToList()
            .AsReadOnly(),
        Services: serviceSchedule.Services
            .Select(ServiceResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record ReservationPolicyResponse(
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    int SlotIntervalMinutes,
    int MaxAdvanceDays,
    IReadOnlyDictionary<ServiceType, TimeSpan> StandardDurations)
{
    public static ReservationPolicyResponse Map(ReservationPolicy policy) => new(
        MinimumAdvanceTime: policy.MinimumAdvanceTime,
        MaximumAdvanceTime: policy.MaximumAdvanceTime,
        SlotInterval: policy.SlotInterval,
        BufferBetweenReservations: policy.BufferBetweenReservations,
        MaxPartySize: policy.MaxPartySize,
        MinPartySize: policy.MinPartySize,
        SlotIntervalMinutes: policy.SlotIntervalMinutes,
        MaxAdvanceDays: policy.MaxAdvanceDays,
        StandardDurations: policy.StandardDurations
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .AsReadOnly());
}

public record ServiceResponse(
    ServiceType Type,
    int MaxCapacity,
    bool HasSpecialDates,
    int AvailableDaysCount,
    IReadOnlyDictionary<DayOfWeek, ServiceDayConfigResponse> WeeklySchedule,
    IReadOnlyCollection<ServiceSpecialDateResponse> SpecialDates)
{
    public static ServiceResponse Map(Service service) => new(
        Type: service.Type,
        MaxCapacity: service.MaxCapacity,
        HasSpecialDates: service.HasSpecialDates,
        AvailableDaysCount: service.AvailableDaysCount,
        WeeklySchedule: service.WeeklySchedule
            .ToDictionary(
                kvp => kvp.Key,
                kvp => ServiceDayConfigResponse.Map(kvp.Value))
            .AsReadOnly(),
        SpecialDates: service.SpecialDates
            .Select(ServiceSpecialDateResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record ServiceDayConfigResponse(
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    TimeSpan? Duration)
{
    public static ServiceDayConfigResponse Map(ServiceDayConfig config) => new(
        IsAvailable: config.IsAvailable,
        StartTime: config.StartTime,
        EndTime: config.EndTime,
        CapacityOverride: config.CapacityOverride,
        Duration: config.Duration);
}

public record ServiceSpecialDateResponse(
    DateOnly Date,
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    string? Reason)
{
    public static ServiceSpecialDateResponse Map(ServiceSpecialDate specialDate) => new(
        Date: specialDate.Date,
        IsAvailable: specialDate.IsAvailable,
        StartTime: specialDate.StartTime,
        EndTime: specialDate.EndTime,
        CapacityOverride: specialDate.CapacityOverride,
        Reason: specialDate.Reason);
}