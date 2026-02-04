namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record ConfigureServiceDayCommand(
    ServiceType Type,
    DayOfWeek Day,
    bool IsAvailable,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int? CapacityOverride = null
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ConfigureServiceDay(
        Service.Create serviceCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<ConfigureServiceDayCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, ConfigureServiceDayCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            var weeklySchedule = existing!.WeeklySchedule.ToDictionary(
                kvp => kvp.Key,
                kvp => new CreateServiceDayConfigCommand(
                    kvp.Value.IsAvailable,
                    kvp.Value.StartTime,
                    kvp.Value.EndTime,
                    kvp.Value.CapacityOverride));

            weeklySchedule[command.Day] = new CreateServiceDayConfigCommand(
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride);

            var updated = serviceCreate.Execute(new CreateServiceCommand(
                existing.Type,
                existing.MaxCapacity,
                weeklySchedule,
                existing.SpecialDates));

            schedule._services.Remove(existing);
            schedule._services.Add(updated);

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}