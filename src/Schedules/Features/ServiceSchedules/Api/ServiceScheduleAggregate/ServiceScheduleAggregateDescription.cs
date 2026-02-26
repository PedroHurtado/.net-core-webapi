namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate;

public class ServiceScheduleAggregateDescription : IAggregateDescription
{
    public string Id => "service-schedule";
    public string DisplayName => "Service Schedules";
    public string? Icon => "calendar";
    public string ReadDescription => "View service schedules";
    public string WriteDescription => "Manage service schedules";
}
