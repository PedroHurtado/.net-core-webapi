namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record ConfigureServiceDayCommand(
    ServiceType Type,
    DayOfWeek Day,
    bool IsAvailable,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int? CapacityOverride = null
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ConfigureServiceDay(
        ServiceDayConfig.Create serviceDayConfigCreate,
        Service.ConfigureDay serviceConfigureDay,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<ConfigureServiceDayCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, ConfigureServiceDayCommand command)
        {
            var service = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(service, $"Service of type '{command.Type}' not found");

            var config = serviceDayConfigCreate.Execute(new CreateServiceDayConfigCommand(
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride));

            serviceConfigureDay.Execute(service!, new ConfigureDayCommand(command.Day, config));

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
