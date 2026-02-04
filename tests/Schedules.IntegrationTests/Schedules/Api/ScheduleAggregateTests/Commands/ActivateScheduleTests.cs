namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class ActivateScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Activate_WithWeeklyHoursConfigured_Returns200()
    {
        // Arrange: Create schedule and configure weekly hours
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var setHoursRequest = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(9, 0), new TimeOnly(17, 0))]);
        await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", setHoursRequest);

        // Act
        var response = await Client.PostAsync($"/schedules/{created.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.PostAsync($"/schedules/{nonExistingId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenAlreadyActive_Returns409()
    {
        // Arrange: Create and activate schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var setHoursRequest = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(9, 0), new TimeOnly(17, 0))]);
        await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", setHoursRequest);

        await Client.PostAsync($"/schedules/{created.Id}/activate", null);

        // Act: Try to activate again
        var response = await Client.PostAsync($"/schedules/{created.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Activate_WithoutWeeklyHours_Returns422()
    {
        // Arrange: Create schedule without weekly hours
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        // Act
        var response = await Client.PostAsync($"/schedules/{created!.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
