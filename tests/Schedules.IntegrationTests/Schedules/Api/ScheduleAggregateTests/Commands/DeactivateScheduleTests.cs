namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class DeactivateScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Deactivate_WhenActive_Returns200()
    {
        // Arrange: Create, configure and activate schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var setHoursRequest = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(9, 0), new TimeOnly(17, 0))]);
        await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", setHoursRequest);
        await Client.PostAsync($"/schedules/{created.Id}/activate", null);

        // Act
        var response = await Client.PostAsync($"/schedules/{created.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.PostAsync($"/schedules/{nonExistingId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_Returns409()
    {
        // Arrange: Create schedule (starts inactive)
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        // Act: Try to deactivate an already inactive schedule
        var response = await Client.PostAsync($"/schedules/{created!.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
