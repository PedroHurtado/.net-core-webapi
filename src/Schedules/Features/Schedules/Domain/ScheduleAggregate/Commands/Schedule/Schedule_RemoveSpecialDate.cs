namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record RemoveSpecialDateCommand(
    DateOnly Date
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSpecialDate(
        IValidator<Schedule> scheduleValidator
    ) : AbstractModifyCommand<RemoveSpecialDateCommand, Schedule>
    {
        public override Schedule Execute(Schedule schedule, RemoveSpecialDateCommand command)
        {
            var existing = schedule.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(existing, $"Special date for '{command.Date}' not found");

            schedule._specialDates.Remove(existing!);

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
