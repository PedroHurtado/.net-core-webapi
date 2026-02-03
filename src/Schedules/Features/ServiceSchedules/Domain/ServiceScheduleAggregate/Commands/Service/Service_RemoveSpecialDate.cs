namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record RemoveServiceSpecialDateCommand(
    DateOnly Date
);

public partial record Service
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSpecialDate(
        IValidator<Service> serviceValidator
    )
    {
        public Service Execute(Service service, RemoveServiceSpecialDateCommand command)
        {
            var existing = service.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(existing, $"Special date '{command.Date}' not found for this service");

            service._specialDates.Remove(existing!);

            return serviceValidator.ValidateOrThrow(service);
        }
    }
}
