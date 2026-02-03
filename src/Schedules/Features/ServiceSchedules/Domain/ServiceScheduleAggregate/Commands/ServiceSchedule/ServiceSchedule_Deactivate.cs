namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule)
        {
            ConflictGuard.ThrowIf(!schedule.IsActive, "Service schedule is already inactive");

            schedule.IsActive = false;

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
