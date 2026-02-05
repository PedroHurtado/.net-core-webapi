namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class ConfigureServiceDayTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task ConfigureServiceDay_WithValidData_Returns204()
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

        var configureRequest = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(12, 0),
            EndTime: new TimeOnly(17, 0),
            CapacityOverride: 60);

        var response = await Client.PutAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Lunch}/days/{DayOfWeek.Friday}",
            configureRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfigureServiceDay_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var configureRequest = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(12, 0),
            EndTime: new TimeOnly(17, 0),
            CapacityOverride: null);

        var response = await Client.PutAsJsonAsync(
            $"/service-schedules/{nonExistingId}/services/{ServiceType.Lunch}/days/{DayOfWeek.Monday}",
            configureRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfigureServiceDay_WithNonExistingService_Returns404()
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

        var configureRequest = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(12, 0),
            EndTime: new TimeOnly(17, 0),
            CapacityOverride: null);

        var response = await Client.PutAsJsonAsync(
            $"/service-schedules/{created!.Id}/services/{ServiceType.Breakfast}/days/{DayOfWeek.Monday}",
            configureRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfigureServiceDay_AvailableWithoutStartTime_Returns422()
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

        var configureRequest = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: null,
            EndTime: new TimeOnly(17, 0),
            CapacityOverride: null);

        var response = await Client.PutAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Lunch}/days/{DayOfWeek.Monday}",
            configureRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
