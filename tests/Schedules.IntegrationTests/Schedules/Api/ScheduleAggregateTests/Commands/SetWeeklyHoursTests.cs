namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class SetWeeklyHoursTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task SetWeeklyHours_WithValidData_Returns204()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await Client.GetAsync($"/schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        updated!.HasWeeklyHours.Should().BeTrue();
        updated.WeeklyHours.Should().ContainKey(DayOfWeek.Monday);
    }

    [Fact]
    public async Task SetWeeklyHours_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();
        var request = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(9, 0), new TimeOnly(17, 0))]);

        var response = await Client.PutAsJsonAsync($"/schedules/{nonExistingId}/weekly-hours/{DayOfWeek.Monday}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetWeeklyHours_WithOverlappingTimeSlots_Returns422()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots:
            [
                new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(15, 0), new TimeOnly(18, 0))
            ]);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetWeeklyHours_WithCloseTimeBeforeOpenTime_Returns422()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(23, 0), new TimeOnly(13, 0))]);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetWeeklyHours_OpenDayWithoutTimeSlots_Returns422()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: []);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}/weekly-hours/{DayOfWeek.Monday}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
