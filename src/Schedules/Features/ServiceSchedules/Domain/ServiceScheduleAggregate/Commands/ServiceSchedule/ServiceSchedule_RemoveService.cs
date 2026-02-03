namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record RemoveServiceCommand(
    ServiceType Type
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveService(
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<RemoveServiceCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, RemoveServiceCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            schedule._services.Remove(existing!);

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
