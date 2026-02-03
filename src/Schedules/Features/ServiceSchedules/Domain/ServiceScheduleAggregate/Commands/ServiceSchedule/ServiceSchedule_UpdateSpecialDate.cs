namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record UpdateServiceScheduleSpecialDateCommand(
    ServiceType Type,
    DateOnly Date,
    bool IsAvailable,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int? CapacityOverride = null,
    string? Reason = null
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateSpecialDate(
        ServiceSpecialDate.Create serviceSpecialDateCreate,
        Service.UpdateSpecialDate serviceUpdateSpecialDate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<UpdateServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, UpdateServiceScheduleSpecialDateCommand command)
        {
            var service = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(service, $"Service of type '{command.Type}' not found");

            var updated = serviceSpecialDateCreate.Execute(new CreateServiceSpecialDateCommand(
                command.Date,
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride,
                command.Reason));

            serviceUpdateSpecialDate.Execute(service!, new UpdateServiceSpecialDateCommand(command.Date, updated));

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
