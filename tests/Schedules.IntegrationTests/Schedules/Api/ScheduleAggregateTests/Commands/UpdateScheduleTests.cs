namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class UpdateScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        // Arrange: Create a schedule first
        var createRequest = new CreateSchedule.Request(Name: "Original Name", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var updateRequest = new UpdateSchedule.Request(
            Name: "Updated Name",
            Description: "New description");

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await Client.GetAsync($"/schedules/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("New description");
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();
        var request = new UpdateSchedule.Request(Name: "Name", Description: null);

        var response = await Client.PutAsJsonAsync($"/schedules/{nonExistingId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_Returns422()
    {
        // Arrange: Create a schedule first
        var createRequest = new CreateSchedule.Request(Name: "Original", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var updateRequest = new UpdateSchedule.Request(Name: "", Description: null);

        // Act
        var response = await Client.PutAsJsonAsync($"/schedules/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
