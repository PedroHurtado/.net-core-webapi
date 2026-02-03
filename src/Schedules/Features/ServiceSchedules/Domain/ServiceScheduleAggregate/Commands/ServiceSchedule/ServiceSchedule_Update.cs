namespace Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate;

public record UpdateServiceScheduleCommand(
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
    public class Update(
        ReservationPolicy.Create reservationPolicyCreate,
        IValidator<ServiceSchedule> serviceScheduleValidator
    ) : AbstractModifyCommand<UpdateServiceScheduleCommand, ServiceSchedule>
    {
        public override ServiceSchedule Execute(ServiceSchedule schedule, UpdateServiceScheduleCommand command)
        {
            var policy = reservationPolicyCreate.Execute(new CreateReservationPolicyCommand(
                command.MinimumAdvanceTime,
                command.MaximumAdvanceTime,
                command.SlotInterval,
                command.BufferBetweenReservations,
                command.MaxPartySize,
                command.MinPartySize,
                command.StandardDurations));

            schedule.Name = command.Name;
            schedule.Description = command.Description;
            schedule.Policy = policy;

            return serviceScheduleValidator.ValidateOrThrow(schedule);
        }
    }
}
