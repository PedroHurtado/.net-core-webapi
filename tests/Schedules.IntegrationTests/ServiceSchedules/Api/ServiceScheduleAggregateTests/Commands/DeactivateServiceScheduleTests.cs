namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class DeactivateServiceScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Deactivate_WhenActive_Returns200()
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
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);
        await Client.PostAsync($"/service-schedules/{created.Id}/activate", null);

        var response = await Client.PostAsync($"/service-schedules/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.PostAsync($"/service-schedules/{nonExistingId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_Returns409()
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

        var response = await Client.PostAsync($"/service-schedules/{created!.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
