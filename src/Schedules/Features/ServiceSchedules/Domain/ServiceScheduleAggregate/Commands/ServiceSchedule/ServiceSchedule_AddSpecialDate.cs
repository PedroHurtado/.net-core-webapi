namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record AddServiceScheduleSpecialDateCommand(
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
    public class AddSpecialDate(
        ServiceSpecialDate.Create serviceSpecialDateCreate,
        Service.AddSpecialDate serviceAddSpecialDate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<AddServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, AddServiceScheduleSpecialDateCommand command)
        {
            var service = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(service, $"Service of type '{command.Type}' not found");

            var specialDate = serviceSpecialDateCreate.Execute(new CreateServiceSpecialDateCommand(
                command.Date,
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride,
                command.Reason));

            serviceAddSpecialDate.Execute(service!, new AddServiceSpecialDateCommand(specialDate));

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
