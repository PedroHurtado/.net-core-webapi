namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record UpdateScheduleCommand(
    string Name,
    string? Description
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<UpdateScheduleCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, UpdateScheduleCommand command)
        {
            schedule.Name = command.Name;
            schedule.Description = command.Description;

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}