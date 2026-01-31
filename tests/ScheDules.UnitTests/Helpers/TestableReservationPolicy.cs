namespace Schedules.UnitTests.Helpers;

public record TestableReservationPolicy(
    TimeSpan minimumAdvanceTime,
    TimeSpan maximumAdvanceTime,
    TimeSpan slotInterval,
    TimeSpan bufferBetweenReservations,
    int maxPartySize,
    int minPartySize
) : ReservationPolicy(minimumAdvanceTime, maximumAdvanceTime, slotInterval, bufferBetweenReservations, maxPartySize, minPartySize)
{
    public Dictionary<ServiceType, TimeSpan> StandardDurationsInternal
    {
        get => _standardDurations;
        set => _standardDurations = value;
    }
}
