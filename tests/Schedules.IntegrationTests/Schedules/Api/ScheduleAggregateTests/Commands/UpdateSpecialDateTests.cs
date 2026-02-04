namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class UpdateSpecialDateTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateSpecialDate_WithValidData_Returns204()
    {
        // Arrange: Create a schedule and add a special date
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var addRequest = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: []);
        await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", addRequest);

        var updateRequest = new UpdateSpecialDate.Request(
            IsClosed: false,
            Reason: "Navidad (horario especial)",
            TimeSlots: [new UpdateSpecialDate.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(18, 0))]);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created.Id}/special-dates/2025-12-25", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await Client.GetAsync($"/schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        var specialDate = updated!.SpecialDates.First(sd => sd.Date == new DateOnly(2025, 12, 25));
        specialDate.IsClosed.Should().BeFalse();
        specialDate.Reason.Should().Be("Navidad (horario especial)");
    }

    [Fact]
    public async Task UpdateSpecialDate_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();
        var request = new UpdateSpecialDate.Request(
            IsClosed: true,
            Reason: "Updated",
            TimeSlots: []);

        var response = await Client.PutAsJsonAsync($"/schedules/{nonExistingId}/special-dates/2025-12-25", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSpecialDate_WithNonExistingDate_Returns404()
    {
        // Arrange: Create a schedule without special dates
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var updateRequest = new UpdateSpecialDate.Request(
            IsClosed: true,
            Reason: "Updated",
            TimeSlots: []);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}/special-dates/2025-08-15", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSpecialDate_WithEmptyReason_Returns422()
    {
        // Arrange: Create a schedule and add a special date
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var addRequest = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: []);
        await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", addRequest);

        var updateRequest = new UpdateSpecialDate.Request(
            IsClosed: true,
            Reason: "",
            TimeSlots: []);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created.Id}/special-dates/2025-12-25", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
