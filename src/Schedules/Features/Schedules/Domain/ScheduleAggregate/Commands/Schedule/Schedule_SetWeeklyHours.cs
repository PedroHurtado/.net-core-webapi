namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record SetWeeklyHoursCommand(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    CreateTimeSlotCommand[] TimeSlots
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SetWeeklyHours(
        DaySchedule.Create dayScheduleCreate,
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<SetWeeklyHoursCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, SetWeeklyHoursCommand command)
        {
            var daySchedule = dayScheduleCreate.Execute(new CreateDayScheduleCommand(
                command.DayOfWeek,
                command.IsClosed,
                command.TimeSlots));

            schedule._weeklyHours[command.DayOfWeek] = daySchedule;

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
