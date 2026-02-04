namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class RemoveSpecialDateTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemoveSpecialDate_WithExistingDate_Returns204()
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

        // Act
        var response = await Client.DeleteAsync($"/schedules/{created.Id}/special-dates/2025-12-25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the removal
        var getResponse = await Client.GetAsync($"/schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        updated!.SpecialDates.Should().NotContain(sd => sd.Date == new DateOnly(2025, 12, 25));
    }

    [Fact]
    public async Task RemoveSpecialDate_WithNonExistingSchedule_Returns404()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/schedules/{nonExistingId}/special-dates/2025-12-25");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSpecialDate_WithNonExistingDate_Returns404()
    {
        // Arrange: Create a schedule without special dates
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        // Act
        var response = await Client.DeleteAsync($"/schedules/{created!.Id}/special-dates/2025-08-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
