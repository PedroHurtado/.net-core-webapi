namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

public record CreateTimeSlotCommand(
    TimeOnly OpenTime,
    TimeOnly CloseTime
);

public partial record TimeSlot
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<TimeSlot> timeSlotValidator
    ) : AbstractCreateCommand<CreateTimeSlotCommand, TimeSlot>
    {
        public override TimeSlot Execute(CreateTimeSlotCommand command)
        {
            var timeSlot = new TimeSlot(
                command.OpenTime,
                command.CloseTime);

            return timeSlotValidator.ValidateOrThrow(timeSlot);
        }
    }
}
