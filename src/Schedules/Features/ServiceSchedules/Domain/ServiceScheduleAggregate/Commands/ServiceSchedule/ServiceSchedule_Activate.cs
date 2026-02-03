namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record ActivateServiceScheduleCommand(
    ServiceSchedule? CurrentActive
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<ActivateServiceScheduleCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, ActivateServiceScheduleCommand command)
        {
            ConflictGuard.ThrowIf(schedule.IsActive, "Service schedule is already active");
            ValidationGuard.ThrowIf(!schedule.HasServices, "Service schedule must have at least one service", nameof(schedule.Services));

            if (command.CurrentActive is not null)
            {
                command.CurrentActive.IsActive = false;
            }

            schedule.IsActive = true;

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
