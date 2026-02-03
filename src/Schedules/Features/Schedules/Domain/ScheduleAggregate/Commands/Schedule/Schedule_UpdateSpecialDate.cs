namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record UpdateSpecialDateCommand(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    CreateTimeSlotCommand[] TimeSlots
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateSpecialDate(
        SpecialDate.Create specialDateCreate,
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<UpdateSpecialDateCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, UpdateSpecialDateCommand command)
        {
            var existing = schedule.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(existing, $"Special date for '{command.Date}' not found");

            var updated = specialDateCreate.Execute(new CreateSpecialDateCommand(
                command.Date,
                command.IsClosed,
                command.Reason,
                command.TimeSlots));

            schedule._specialDates.Remove(existing);
            schedule._specialDates.Add(updated);

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
