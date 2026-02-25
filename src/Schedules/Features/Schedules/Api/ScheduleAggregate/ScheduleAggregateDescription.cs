namespace Schedules.Features.Schedules.Api.ScheduleAggregate;

public class ScheduleAggregateDescription : IAggregateDescription
{
    public string Id => "schedule";
    public string DisplayName => "Horarios";
    public string? Icon => "clock";
}
