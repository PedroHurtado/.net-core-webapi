namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario Original",
            Description: "Description Original",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var updateRequest = new UpdateServiceSchedule.Request(
            Name: "Horario Actualizado",
            Description: "Description Actualizada",
            MinimumAdvanceTime: TimeSpan.FromHours(3),
            MaximumAdvanceTime: TimeSpan.FromDays(60),
            SlotInterval: TimeSpan.FromMinutes(15),
            BufferBetweenReservations: TimeSpan.FromMinutes(10),
            MaxPartySize: 10,
            MinPartySize: 2,
            StandardDurations: []);

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new UpdateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PutAsJsonAsync($"/service-schedules/{nonExistingId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_Returns422()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario Original",
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

        var updateRequest = new UpdateServiceSchedule.Request(
            Name: "",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithInvalidPolicy_Returns422()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario Original",
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

        var updateRequest = new UpdateServiceSchedule.Request(
            Name: "Horario",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(5),
            MaximumAdvanceTime: TimeSpan.FromHours(2),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PutAsJsonAsync($"/service-schedules/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithStandardDurations_PersistsDurations()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario Original",
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

        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        var updateRequest = new UpdateServiceSchedule.Request(
            Name: "Horario Actualizado",
            Description: "Description Actualizada",
            MinimumAdvanceTime: TimeSpan.FromHours(3),
            MaximumAdvanceTime: TimeSpan.FromDays(60),
            SlotInterval: TimeSpan.FromMinutes(15),
            BufferBetweenReservations: TimeSpan.FromMinutes(10),
            MaxPartySize: 10,
            MinPartySize: 2,
            StandardDurations: standardDurations);

        var updateResponse = await Client.PutAsJsonAsync($"/service-schedules/{created!.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/service-schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        updated!.Policy.StandardDurations.Should().HaveCount(3);
        updated.Policy.StandardDurations[ServiceType.Breakfast].Should().Be(TimeSpan.FromHours(1));
        updated.Policy.StandardDurations[ServiceType.Lunch].Should().Be(TimeSpan.FromMinutes(90));
        updated.Policy.StandardDurations[ServiceType.Dinner].Should().Be(TimeSpan.FromHours(2));
    }
}
