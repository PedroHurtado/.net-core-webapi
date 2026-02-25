namespace Schedules.Features.Schedules.Api.ScheduleAggregate;

public class ScheduleAggregateDescription : IAggregateDescription
{
    public string Id => "schedule";
    public string DisplayName => "Horarios";
    public string? Icon => "clock";
    public string ReadDescription => "View schedules";
    public string WriteDescription => "Manage schedules";
}
