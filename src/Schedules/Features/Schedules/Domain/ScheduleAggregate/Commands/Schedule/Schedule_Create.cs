namespace Schedules.Features.Schedules.Domain.ScheduleAggregate;

public record CreateScheduleCommand(
    Guid TenantId,
    string Name,
    string? Description
);

public partial class Schedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Schedule> scheduleValidator
    ) : AbstractCreateCommand<CreateScheduleCommand, Schedule>
    {
        public override Schedule Execute(CreateScheduleCommand command)
        {
            var schedule = new Schedule(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = command.Name,
                Description = command.Description,
                IsActive = false
            };

            return scheduleValidator.ValidateOrThrow(schedule);
        }
    }
}