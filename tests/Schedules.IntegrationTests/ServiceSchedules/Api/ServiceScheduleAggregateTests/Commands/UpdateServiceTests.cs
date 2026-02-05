namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateService_WithValidData_Returns204()
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

        var updateServiceRequest = new UpdateService.Request(
            MaxCapacity: 60,
            WeeklySchedule: new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(17, 0), 70) },
                { DayOfWeek.Tuesday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created.Id}/services/{ServiceType.Lunch}", updateServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateService_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var updateServiceRequest = new UpdateService.Request(
            MaxCapacity: 60,
            WeeklySchedule: new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PutAsJsonAsync($"/service-schedules/{nonExistingId}/services/{ServiceType.Lunch}", updateServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateService_WithNonExistingService_Returns404()
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

        var updateServiceRequest = new UpdateService.Request(
            MaxCapacity: 60,
            WeeklySchedule: new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created!.Id}/services/{ServiceType.Breakfast}", updateServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateService_WithInvalidData_Returns422()
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

        var updateServiceRequest = new UpdateService.Request(
            MaxCapacity: 0,
            WeeklySchedule: new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
            });

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created.Id}/services/{ServiceType.Lunch}", updateServiceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
