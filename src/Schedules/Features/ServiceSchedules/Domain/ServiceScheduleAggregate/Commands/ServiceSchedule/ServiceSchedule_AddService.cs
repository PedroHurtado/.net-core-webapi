namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record AddServiceCommand(
    ServiceType Type,
    int MaxCapacity,
    Dictionary<DayOfWeek, CreateServiceDayConfigCommand> WeeklySchedule
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddService(
        Service.Create serviceCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<AddServiceCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, AddServiceCommand command)
        {
            ConflictGuard.ThrowIf(
                schedule.Services.Any(s => s.Type == command.Type),
                $"Service of type '{command.Type}' already exists");

            var service = serviceCreate.Execute(new CreateServiceCommand(
                command.Type,
                command.MaxCapacity,
                command.WeeklySchedule));

            schedule._services.Add(service);

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
