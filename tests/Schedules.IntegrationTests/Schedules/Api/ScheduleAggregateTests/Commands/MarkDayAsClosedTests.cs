namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class MarkDayAsClosedTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task MarkDayAsClosed_WithValidData_Returns204()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        // Act
        var response = await Client.PostAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Sunday}/close", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await Client.GetAsync($"/schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        updated!.WeeklyHours.Should().ContainKey(DayOfWeek.Sunday);
        updated.WeeklyHours[DayOfWeek.Sunday].IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task MarkDayAsClosed_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.PostAsync($"/schedules/{nonExistingId}/weekly-hours/{DayOfWeek.Sunday}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
