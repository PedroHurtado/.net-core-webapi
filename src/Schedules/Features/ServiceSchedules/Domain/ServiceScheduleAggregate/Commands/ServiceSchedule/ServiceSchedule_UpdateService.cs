namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record UpdateServiceCommand(
    ServiceType Type,
    int MaxCapacity,
    Dictionary<DayOfWeek, CreateServiceDayConfigCommand> WeeklySchedule
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateService(
        Service.Create serviceCreate,
        Service.AddSpecialDate serviceAddSpecialDate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<UpdateServiceCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, UpdateServiceCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            var updated = serviceCreate.Execute(new CreateServiceCommand(
                command.Type,
                command.MaxCapacity,
                command.WeeklySchedule));

            foreach (var specialDate in existing!.SpecialDates)
            {
                serviceAddSpecialDate.Execute(updated, new AddServiceSpecialDateCommand(specialDate));
            }

            schedule._services.Remove(existing);
            schedule._services.Add(updated);

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
