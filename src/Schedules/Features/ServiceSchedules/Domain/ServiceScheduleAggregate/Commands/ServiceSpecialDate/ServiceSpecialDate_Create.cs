namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record CreateServiceSpecialDateCommand(
    DateOnly Date,
    bool IsAvailable,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int? CapacityOverride = null,
    string? Reason = null
);

public partial record ServiceSpecialDate
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ServiceSpecialDate> serviceSpecialDateValidator
    ) : AbstractCreateCommand<CreateServiceSpecialDateCommand, ServiceSpecialDate>
    {
        public override ServiceSpecialDate Execute(CreateServiceSpecialDateCommand command)
        {
            var specialDate = new ServiceSpecialDate(
                command.Date,
                command.IsAvailable,
                command.StartTime,
                command.EndTime,
                command.CapacityOverride,
                command.Reason);

            return serviceSpecialDateValidator.ValidateOrThrow(specialDate);
        }
    }
}
