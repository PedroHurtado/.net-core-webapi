namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate;

public class ServiceScheduleAggregateDescription : IAggregateDescription
{
    public string Id => "service-schedule";
    public string DisplayName => "Horarios de Servicio";
    public string? Icon => "calendar";
}
