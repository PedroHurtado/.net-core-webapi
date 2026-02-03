namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record CreateServiceScheduleCommand(
    Guid TenantId,
    string Name,
    string Description,
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    Dictionary<ServiceType, TimeSpan> StandardDurations
);

public partial class ServiceSchedule
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        ReservationPolicy.Create reservationPolicyCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractCreateCommand<CreateServiceScheduleCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(CreateServiceScheduleCommand command)
        {
            var policy = reservationPolicyCreate.Execute(new CreateReservationPolicyCommand(
                command.MinimumAdvanceTime,
                command.MaximumAdvanceTime,
                command.SlotInterval,
                command.BufferBetweenReservations,
                command.MaxPartySize,
                command.MinPartySize,
                command.StandardDurations));

            var schedule = new ServiceSchedule(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = command.Name,
                Description = command.Description,
                IsActive = false,
                Policy = policy
            };

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
