namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Queries;

public class GetSchedulesTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task List_WithExistingSchedules_Returns200()
    {
        // Arrange: Create schedules
        var request1 = new CreateSchedule.Request(Name: "Schedule A", Description: null);
        var request2 = new CreateSchedule.Request(Name: "Schedule B", Description: null);
        await Client.PostAsJsonAsync("/schedules", request1);
        await Client.PostAsJsonAsync("/schedules", request2);

        // Act
        var response = await Client.GetAsync("/schedules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ScheduleResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task List_WithNoSchedules_Returns200WithEmptyArray()
    {
        // Act
        var response = await Client.GetAsync("/schedules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ScheduleResponse>>(JsonOptions);
        result.Should().NotBeNull();
    }
}
