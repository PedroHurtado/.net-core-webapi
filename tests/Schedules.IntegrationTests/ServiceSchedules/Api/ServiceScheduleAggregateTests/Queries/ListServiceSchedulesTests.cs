namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class ListServiceSchedulesTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task List_ReturnsAllSchedules_Returns200()
    {
        var createRequest1 = new CreateServiceSchedule.Request(
            Name: "Schedule 1",
            Description: "Description 1",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var createRequest2 = new CreateServiceSchedule.Request(
            Name: "Schedule 2",
            Description: "Description 2",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        await Client.PostAsJsonAsync("/service-schedules", createRequest1);
        await Client.PostAsJsonAsync("/service-schedules", createRequest2);

        var response = await Client.GetAsync("/service-schedules");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceScheduleResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task List_FilterByIsActive_Returns200()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Schedule Activo",
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

        var response = await Client.GetAsync("/service-schedules?isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceScheduleResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task List_WhenEmpty_ReturnsEmptyArray_Returns200()
    {
        var response = await Client.GetAsync("/service-schedules");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceScheduleResponse>>(JsonOptions);
        result.Should().NotBeNull();
    }
}
