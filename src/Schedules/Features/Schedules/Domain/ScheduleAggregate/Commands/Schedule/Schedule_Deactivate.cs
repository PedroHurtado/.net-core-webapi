namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<Schedule>
    {
        public override Schedule Execute(Schedule schedule)
        {
            ConflictGuard.ThrowIf(!schedule.IsActive, "Schedule is already inactive");

            schedule.IsActive = false;

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
