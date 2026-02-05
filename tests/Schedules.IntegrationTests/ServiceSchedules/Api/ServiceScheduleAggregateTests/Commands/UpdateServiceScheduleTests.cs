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
}
