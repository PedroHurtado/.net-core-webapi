namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record RemoveServiceScheduleSpecialDateCommand(
    ServiceType Type,
    DateOnly Date
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSpecialDate(
        Service.RemoveSpecialDate serviceRemoveSpecialDate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<RemoveServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, RemoveServiceScheduleSpecialDateCommand command)
        {
            var service = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(service, $"Service of type '{command.Type}' not found");

            serviceRemoveSpecialDate.Execute(service!, new RemoveServiceSpecialDateCommand(command.Date));

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
