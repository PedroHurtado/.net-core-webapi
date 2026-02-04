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
        Service.Create serviceCreate,
        ServiceSpecialDate.Create serviceSpecialDateCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<AddServiceScheduleSpecialDateCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, AddServiceScheduleSpecialDateCommand command)
        {
            var existing = schedule.Services.FirstOrDefault(s => s.Type == command.Type);
            NotFoundGuard.ThrowIfNull(existing, $"Service of type '{command.Type}' not found");

            ConflictGuard.ThrowIf(
                existing!.SpecialDates.Any(sd => sd.Date == command.Date),
                $"Special date '{command.Date}' already exists for this service");

            var newSpecialDate = serviceSpecialDateCreate.Execute(new CreateServiceSpecialDateCommand(
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

            var updatedSpecialDates = existing.SpecialDates.Append(newSpecialDate);

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
