namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record RemoveServiceScheduleSpecialDateCommand(
    ServiceType Type,
    DateOnly Date
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSpecialDate(
        Service.Create serviceCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<RemoveServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, RemoveServiceScheduleSpecialDateCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            var specialDateToRemove = existing!.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(specialDateToRemove, $"Special date '{command.Date}' not found for this service");

            var weeklySchedule = existing.WeeklySchedule.ToDictionary(
                kvp => kvp.Key,
                kvp => new CreateServiceDayConfigCommand(
                    kvp.Value.IsAvailable,
                    kvp.Value.StartTime,
                    kvp.Value.EndTime,
                    kvp.Value.CapacityOverride));

            var updatedSpecialDates = existing.SpecialDates.Where(sd => sd.Date != command.Date);

            var updated = serviceCreate.Execute(new CreateServiceCommand(
                existing.Type,
                existing.MaxCapacity,
                weeklySchedule,
                updatedSpecialDates));

            schedule._services.Remove(existing);
            schedule._services.Add(updated);

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
