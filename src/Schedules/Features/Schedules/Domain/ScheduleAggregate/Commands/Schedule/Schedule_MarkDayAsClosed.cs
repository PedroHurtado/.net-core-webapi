namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record MarkDayAsClosedCommand(
    DayOfWeek DayOfWeek
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class MarkDayAsClosed(
        DaySchedule.Create dayScheduleCreate,
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<MarkDayAsClosedCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, MarkDayAsClosedCommand command)
        {
            var daySchedule = dayScheduleCreate.Execute(new CreateDayScheduleCommand(
                command.DayOfWeek,
                IsClosed: true,
                TimeSlots: []));

            schedule._weeklyHours[command.DayOfWeek] = daySchedule;

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
