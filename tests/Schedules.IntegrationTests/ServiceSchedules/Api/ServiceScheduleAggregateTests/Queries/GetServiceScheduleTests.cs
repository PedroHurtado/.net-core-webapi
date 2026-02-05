namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class GetServiceScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Get_WithExistingId_Returns200()
    {
        var createRequest = new CreateServiceSchedule.Request(
            Name: "Horario Verano",
            Description: "Junio a Septiembre",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var createResponse = await Client.PostAsJsonAsync("/service-schedules", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);

        var response = await Client.GetAsync($"/service-schedules/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Horario Verano");
    }

    [Fact]
    public async Task Get_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/service-schedules/{nonExistingId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
