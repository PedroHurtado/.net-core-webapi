namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class AddServiceTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddService_WithValidData_Returns201()
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
                { DayOfWeek.Tuesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Wednesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Thursday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Friday, new AddService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(17, 0), 60) },
                { DayOfWeek.Saturday, new AddService.ServiceDayConfigInput(false, null, null, null) },
                { DayOfWeek.Sunday, new AddService.ServiceDayConfigInput(false, null, null, null) }
            });

        var response = await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.HasServices.Should().BeTrue();
        result.ServiceCount.Should().Be(1);
        result.Services.Should().ContainSingle(s => s.Type == ServiceType.Lunch);
    }

    [Fact]
    public async Task AddService_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var addServiceRequest = new AddService.Request(
            Type: ServiceType.Lunch,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PostAsJsonAsync($"/service-schedules/{nonExistingId}/services", addServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddService_WithDuplicateServiceType_Returns409()
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

        var response = await Client.PostAsJsonAsync($"/service-schedules/{created.Id}/services", addServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddService_WithZeroMaxCapacity_Returns422()
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
            MaxCapacity: 0,
            WeeklySchedule: new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddService_WithNoDaysAvailable_Returns422()
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
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(false, null, null, null) },
                { DayOfWeek.Tuesday, new AddService.ServiceDayConfigInput(false, null, null, null) }
            });

        var response = await Client.PostAsJsonAsync($"/service-schedules/{created!.Id}/services", addServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
