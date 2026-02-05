namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class RemoveSpecialDateTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemoveSpecialDate_WithExistingDate_Returns204()
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
            Type: ServiceType.Dinner,
            MaxCapacity: 40,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var addSpecialDateRequest = new AddServiceScheduleSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsAvailable: true,
            StartTime: new TimeOnly(19, 0),
            EndTime: new TimeOnly(23, 0),
            CapacityOverride: null,
            Reason: "San Valentin");

        await Client.PostAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates",
            addSpecialDateRequest);

        var response = await Client.DeleteAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates/2025-02-14");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveSpecialDate_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.DeleteAsync(
            $"/service-schedules/{nonExistingId}/services/{ServiceType.Dinner}/special-dates/2025-02-14");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSpecialDate_WithNonExistingService_Returns404()
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

        var response = await Client.DeleteAsync(
            $"/service-schedules/{created!.Id}/services/{ServiceType.Breakfast}/special-dates/2025-02-14");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSpecialDate_WithNonExistingDate_Returns404()
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
            Type: ServiceType.Dinner,
            MaxCapacity: 40,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        var response = await Client.DeleteAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates/2025-03-15");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
