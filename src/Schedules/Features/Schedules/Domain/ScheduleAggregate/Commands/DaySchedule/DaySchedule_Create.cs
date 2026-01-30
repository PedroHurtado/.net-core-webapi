namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

public record CreateDayScheduleCommand(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    CreateTimeSlotCommand[] TimeSlots
);

public partial record DaySchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        TimeSlot.Create timeSlotCreate,
        IValidator<DaySchedule> dayScheduleValidator
    ) : AbstractCreateCommand<CreateDayScheduleCommand, DaySchedule>
    {
        public override DaySchedule Execute(CreateDayScheduleCommand command)
        {
            var timeSlots = command.TimeSlots
                .Select(ts => timeSlotCreate.Execute(ts))
                .ToArray();

            var daySchedule = new DaySchedule(
                command.DayOfWeek,
                command.IsClosed,
                timeSlots);

            return dayScheduleValidator.ValidateOrThrow(daySchedule);
        }
    }
}
