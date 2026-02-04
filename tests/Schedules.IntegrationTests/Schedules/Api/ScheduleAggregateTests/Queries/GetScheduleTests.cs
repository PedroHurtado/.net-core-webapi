namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Queries;

public class GetScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Get_WithExistingId_Returns200()
    {
        // Arrange: Create a schedule first
        var createRequest = new CreateSchedule.Request(
            Name: "Test Schedule",
            Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/schedules/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Test Schedule");
    }

    [Fact]
    public async Task Get_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/schedules/{nonExistingId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
