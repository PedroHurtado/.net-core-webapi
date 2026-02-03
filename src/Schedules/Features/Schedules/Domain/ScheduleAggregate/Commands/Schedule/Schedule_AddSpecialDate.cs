namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record AddSpecialDateCommand(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    CreateTimeSlotCommand[] TimeSlots
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddSpecialDate(
        SpecialDate.Create specialDateCreate,
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<AddSpecialDateCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, AddSpecialDateCommand command)
        {
            ConflictGuard.ThrowIf(
                schedule.SpecialDates.Any(sd => sd.Date == command.Date),
                $"Special date for '{command.Date}' already exists");

            var specialDate = specialDateCreate.Execute(new CreateSpecialDateCommand(
                command.Date,
                command.IsClosed,
                command.Reason,
                command.TimeSlots));

            schedule._specialDates.Add(specialDate);

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
