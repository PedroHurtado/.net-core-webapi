namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class AddSpecialDateTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddSpecialDate_WithValidData_Returns201()
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
            EndTime: new TimeOnly(23, 30),
            CapacityOverride: 70,
            Reason: "San Valentin");

        var response = await Client.PostAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates",
            addSpecialDateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        var service = result!.Services.First(s => s.Type == ServiceType.Dinner);
        service.HasSpecialDates.Should().BeTrue();
        service.SpecialDates.Should().ContainSingle(sd => sd.Date == new DateOnly(2025, 2, 14));
    }

    [Fact]
    public async Task AddSpecialDate_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var addSpecialDateRequest = new AddServiceScheduleSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsAvailable: true,
            StartTime: new TimeOnly(19, 0),
            EndTime: new TimeOnly(23, 0),
            CapacityOverride: null,
            Reason: null);

        var response = await Client.PostAsJsonAsync(
            $"/service-schedules/{nonExistingId}/services/{ServiceType.Dinner}/special-dates",
            addSpecialDateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSpecialDate_WithNonExistingService_Returns404()
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

        var addSpecialDateRequest = new AddServiceScheduleSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsAvailable: true,
            StartTime: new TimeOnly(19, 0),
            EndTime: new TimeOnly(23, 0),
            CapacityOverride: null,
            Reason: null);

        var response = await Client.PostAsJsonAsync(
            $"/service-schedules/{created!.Id}/services/{ServiceType.Breakfast}/special-dates",
            addSpecialDateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSpecialDate_WithDuplicateDate_Returns409()
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

        var response = await Client.PostAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates",
            addSpecialDateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddSpecialDate_WithInvalidData_Returns422()
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
            StartTime: null,
            EndTime: new TimeOnly(23, 0),
            CapacityOverride: null,
            Reason: null);

        var response = await Client.PostAsJsonAsync(
            $"/service-schedules/{created.Id}/services/{ServiceType.Dinner}/special-dates",
            addSpecialDateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
