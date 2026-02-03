namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record ConfigureDayCommand(
    DayOfWeek Day,
    ServiceDayConfig Config
);

public partial record Service
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ConfigureDay(
        IValidator<Service> serviceValidator
    )
    {
        public Service Execute(Service service, ConfigureDayCommand command)
        {
            service._weeklySchedule[command.Day] = command.Config;

            return serviceValidator.ValidateOrThrow(service);
        }
    }
}
