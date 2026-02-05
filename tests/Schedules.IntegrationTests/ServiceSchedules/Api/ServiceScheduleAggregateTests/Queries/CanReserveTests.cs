namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class CanReserveTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task CanReserve_WithValidReservation_ReturnsTrue()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) }
            });

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var addServiceRequest = new AddService.Request(
            Type: ServiceType.Lunch,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Tuesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Wednesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Thursday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Friday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var futureDate = DateTime.UtcNow.AddDays(7);
        while (futureDate.DayOfWeek == DayOfWeek.Saturday || futureDate.DayOfWeek == DayOfWeek.Sunday)
        {
            futureDate = futureDate.AddDays(1);
        }
        var reservationTime = new DateTime(futureDate.Year, futureDate.Month, futureDate.Day, 13, 30, 0);

        var response = await Client.GetAsync(
            $"/service-schedules/{created.Id}/can-reserve?type={ServiceType.Lunch}&dateTime={reservationTime:O}&partySize=4");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CanReserve.Response>(JsonOptions);
        result.Should().NotBeNull();
        result!.CanReserve.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [Fact]
    public async Task CanReserve_ServiceNotAvailable_ReturnsFalseWithReason()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var addServiceRequest = new AddService.Request(
            Type: ServiceType.Lunch,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Sunday, new AddService.ServiceDayConfigInput(false, null, null, null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var futureDate = DateTime.UtcNow.AddDays(7);
        while (futureDate.DayOfWeek != DayOfWeek.Sunday)
        {
            futureDate = futureDate.AddDays(1);
        }
        var reservationTime = new DateTime(futureDate.Year, futureDate.Month, futureDate.Day, 13, 30, 0);

        var response = await Client.GetAsync(
            $"/service-schedules/{created.Id}/can-reserve?type={ServiceType.Lunch}&dateTime={reservationTime:O}&partySize=4");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CanReserve.Response>(JsonOptions);
        result.Should().NotBeNull();
        result!.CanReserve.Should().BeFalse();
        result.Reason.Should().Contain("not available");
    }

    [Fact]
    public async Task CanReserve_InvalidPartySize_ReturnsFalseWithReason()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) }
            });

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var addServiceRequest = new AddService.Request(
            Type: ServiceType.Lunch,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var futureDate = DateTime.UtcNow.AddDays(7);
        while (futureDate.DayOfWeek != DayOfWeek.Monday)
        {
            futureDate = futureDate.AddDays(1);
        }
        var reservationTime = new DateTime(futureDate.Year, futureDate.Month, futureDate.Day, 13, 30, 0);

        var response = await Client.GetAsync(
            $"/service-schedules/{created.Id}/can-reserve?type={ServiceType.Lunch}&dateTime={reservationTime:O}&partySize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CanReserve.Response>(JsonOptions);
        result.Should().NotBeNull();
        result!.CanReserve.Should().BeFalse();
        result.Reason.Should().Contain("Maximum");
    }

    [Fact]
    public async Task CanReserve_InvalidSlot_ReturnsFalseWithReason()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) }
            });

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var addServiceRequest = new AddService.Request(
            Type: ServiceType.Lunch,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var futureDate = DateTime.UtcNow.AddDays(7);
        while (futureDate.DayOfWeek != DayOfWeek.Monday)
        {
            futureDate = futureDate.AddDays(1);
        }
        var reservationTime = new DateTime(futureDate.Year, futureDate.Month, futureDate.Day, 13, 17, 0);

        var response = await Client.GetAsync(
            $"/service-schedules/{created.Id}/can-reserve?type={ServiceType.Lunch}&dateTime={reservationTime:O}&partySize=4");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CanReserve.Response>(JsonOptions);
        result.Should().NotBeNull();
        result!.CanReserve.Should().BeFalse();
        result.Reason.Should().Contain("Reservations only accepted");
    }

    [Fact]
    public async Task CanReserve_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();
        var reservationTime = DateTime.UtcNow.AddDays(7);

        var response = await Client.GetAsync(
            $"/service-schedules/{nonExistingId}/can-reserve?type={ServiceType.Lunch}&dateTime={reservationTime:O}&partySize=4");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
