namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

public record CreateDayScheduleCommand(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    TimeSlot[] TimeSlots
);

public partial record DaySchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<DaySchedule> dayScheduleValidator
    ) : AbstractCreateCommand<CreateDayScheduleCommand, DaySchedule>
    {
        public override DaySchedule Execute(CreateDayScheduleCommand command)
        {
            var daySchedule = new DaySchedule(
                command.DayOfWeek,
                command.IsClosed,
                command.TimeSlots);

            return dayScheduleValidator.ValidateOrThrow(daySchedule);
        }
    }
}
