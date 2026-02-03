namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record CreateServiceDayConfigCommand(
    bool IsAvailable,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int? CapacityOverride = null
);

public partial record ServiceDayConfig
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ServiceDayConfig> serviceDayConfigValidator
    ) : AbstractCreateCommand<CreateServiceDayConfigCommand, ServiceDayConfig>
    {
        public override ServiceDayConfig Execute(CreateServiceDayConfigCommand command)
        {
            var config = new ServiceDayConfig(
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride);

            return serviceDayConfigValidator.ValidateOrThrow(config);
        }
    }
}
