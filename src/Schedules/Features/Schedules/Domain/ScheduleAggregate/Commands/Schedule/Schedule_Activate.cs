namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record ActivateScheduleCommand(
    Schedule? CurrentActive
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<ActivateScheduleCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, ActivateScheduleCommand command)
        {
            ConflictGuard.ThrowIf(schedule.IsActive, "Schedule is already active");
            ValidationGuard.ThrowIf(!schedule.HasWeeklyHours, "Schedule must have at least one day configured", nameof(schedule.WeeklyHours));

            if (command.CurrentActive is not null)
            {
                command.CurrentActive.IsActive = false;
            }

            schedule.IsActive = true;

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
