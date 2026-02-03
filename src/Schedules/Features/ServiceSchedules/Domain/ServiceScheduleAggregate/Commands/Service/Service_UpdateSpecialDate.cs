namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record UpdateServiceSpecialDateCommand(
    DateOnly Date,
    ServiceSpecialDate UpdatedSpecialDate
);

public partial record Service
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateSpecialDate(
        IValidator<Service> serviceValidator
    )
    {
        public Service Execute(Service service, UpdateServiceSpecialDateCommand command)
        {
            var existing = service.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(existing, $"Special date '{command.Date}' not found for this service");

            service._specialDates.Remove(existing!);
            service._specialDates.Add(command.UpdatedSpecialDate);

            return serviceValidator.ValidateOrThrow(service);
        }
    }
}
