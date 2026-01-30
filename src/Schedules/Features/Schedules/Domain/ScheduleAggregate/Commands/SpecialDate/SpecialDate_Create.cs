namespace Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

public record CreateSpecialDateCommand(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    CreateTimeSlotCommand[] TimeSlots
);

public partial record SpecialDate
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        TimeSlot.Create timeSlotCreate,
        IValidator<SpecialDate> specialDateValidator
    ) : AbstractCreateCommand<CreateSpecialDateCommand, SpecialDate>
    {
        public override SpecialDate Execute(CreateSpecialDateCommand command)
        {
            var timeSlots = command.TimeSlots
                .Select(ts => timeSlotCreate.Execute(ts))
                .ToArray();

            var specialDate = new SpecialDate(
                command.Date,
                command.IsClosed,
                command.Reason,
                timeSlots);

            return specialDateValidator.ValidateOrThrow(specialDate);
        }
    }
}
