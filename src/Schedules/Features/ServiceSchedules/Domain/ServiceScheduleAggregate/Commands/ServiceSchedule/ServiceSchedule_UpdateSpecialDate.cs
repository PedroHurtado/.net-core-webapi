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
        Service.Create serviceCreate,
        ServiceSpecialDate.Create serviceSpecialDateCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<UpdateServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, UpdateServiceScheduleSpecialDateCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            var existingSpecialDate = existing!.SpecialDates.FirstOrDefault(sd => sd.Date == command.Date);
            NotFoundGuard.ThrowIfNull(existingSpecialDate, $"Special date '{command.Date}' not found for this service");

            var updatedSpecialDate = serviceSpecialDateCreate.Execute(new CreateServiceSpecialDateCommand(
                command.Date,
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride,
                command.Reason));

            var weeklySchedule = existing.WeeklySchedule.ToDictionary(
                kvp => kvp.Key,
                kvp => new CreateServiceDayConfigCommand(
                    kvp.Value.IsAvailable,
                    kvp.Value.StartTime,
                    kvp.Value.EndTime,
                    kvp.Value.CapacityOverride));

            var updatedSpecialDates = existing.SpecialDates
                .Where(sd => sd.Date != command.Date)
                .Append(updatedSpecialDate);

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
