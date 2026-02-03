namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record AddServiceSpecialDateCommand(
    ServiceSpecialDate SpecialDate
);

public partial record Service
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddSpecialDate(
        IValidator<Service> serviceValidator
    )
    {
        public Service Execute(Service service, AddServiceSpecialDateCommand command)
        {
            ConflictGuard.ThrowIf(
                service.SpecialDates.Any(sd => sd.Date == command.SpecialDate.Date),
                $"Special date '{command.SpecialDate.Date}' already exists for this service");

            service._specialDates.Add(command.SpecialDate);

            return serviceValidator.ValidateOrThrow(service);
        }
    }
}
