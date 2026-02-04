namespace Schedules.UnitTests.Helpers;

public static class ServiceScheduleTestHelper
{
    public static ServiceScheduleResponse CreateMockResponse(Guid? id = null)
    {
        return new ServiceScheduleResponse(
            Id: id ?? Guid.NewGuid(),
            Name: "Test",
            Description: "Test",
            IsActive: false,
            Policy: new ReservationPolicyResponse(
                MinimumAdvanceTime: TimeSpan.FromHours(2),
                MaximumAdvanceTime: TimeSpan.FromDays(30),
                SlotInterval: TimeSpan.FromMinutes(30),
                BufferBetweenReservations: TimeSpan.FromMinutes(15),
                MaxPartySize: 8,
                MinPartySize: 1,
                SlotIntervalMinutes: 30,
                MaxAdvanceDays: 30,
                StandardDurations: new Dictionary<ServiceType, TimeSpan>().AsReadOnly()),
            HasServices: false,
            ServiceCount: 0,
            AvailableServiceTypes: Array.Empty<ServiceType>(),
            Services: Array.Empty<ServiceResponse>());
    }
}
