namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record CreateServiceCommand(
    ServiceType Type,
    int MaxCapacity,
    Dictionary<DayOfWeek, CreateServiceDayConfigCommand> WeeklySchedule
);

public partial record Service
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        ServiceDayConfig.Create serviceDayConfigCreate,
        IValidator<Service> serviceValidator
    ) : AbstractCreateCommand<CreateServiceCommand, Service>
    {
        public override Service Execute(CreateServiceCommand command)
        {
            var service = new Service(command.Type, command.MaxCapacity);

            foreach (var (day, configCommand) in command.WeeklySchedule)
            {
                var config = serviceDayConfigCreate.Execute(configCommand);
                service._weeklySchedule[day] = config;
            }

            return serviceValidator.ValidateOrThrow(service);
        }
    }
}
