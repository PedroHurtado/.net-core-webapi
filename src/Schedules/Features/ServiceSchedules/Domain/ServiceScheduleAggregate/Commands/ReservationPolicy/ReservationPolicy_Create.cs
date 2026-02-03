namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;

public record CreateReservationPolicyCommand(
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    Dictionary<ServiceType, TimeSpan> StandardDurations
);

public partial record ReservationPolicy
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ReservationPolicy> reservationPolicyValidator
    ) : AbstractCreateCommand<CreateReservationPolicyCommand, ReservationPolicy>
    {
        public override ReservationPolicy Execute(CreateReservationPolicyCommand command)
        {
            var policy = new ReservationPolicy(
                command.MinimumAdvanceTime,
                command.MaximumAdvanceTime,
                command.SlotInterval,
                command.BufferBetweenReservations,
                command.MaxPartySize,
                command.MinPartySize);

            foreach (var (serviceType, duration) in command.StandardDurations)
            {
                policy._standardDurations[serviceType] = duration;
            }

            return reservationPolicyValidator.ValidateOrThrow(policy);
        }
    }
}