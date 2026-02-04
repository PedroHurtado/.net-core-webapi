namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class AddSpecialDateTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddSpecialDate_ClosedDate_Returns201()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: []);

        // Act
        var response = await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        result!.HasSpecialDates.Should().BeTrue();
        result.SpecialDates.Should().Contain(sd => sd.Date == new DateOnly(2025, 12, 25) && sd.IsClosed);
    }

    [Fact]
    public async Task AddSpecialDate_WithNonExistingId_Returns404()
    {
        var nonExistingId = Guid.NewGuid();
        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: []);

        var response = await Client.PostAsJsonAsync($"/schedules/{nonExistingId}/special-dates", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSpecialDate_DuplicateDate_Returns409()
    {
        // Arrange: Create a schedule and add a special date
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: []);
        await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", request);

        // Act: Try to add the same date
        var duplicateRequest = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Another reason",
            TimeSlots: []);
        var response = await Client.PostAsJsonAsync($"/schedules/{created.Id}/special-dates", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddSpecialDate_WithEmptyReason_Returns422()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "",
            TimeSlots: []);

        // Act
        var response = await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddSpecialDate_OpenDateWithoutTimeSlots_Returns422()
    {
        // Arrange: Create a schedule
        var createRequest = new CreateSchedule.Request(Name: "Test Schedule", Description: null);
        var createResponse = await Client.PostAsJsonAsync("/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsClosed: false,
            Reason: "San Valentín",
            TimeSlots: []);

        // Act
        var response = await Client.PostAsJsonAsync($"/schedules/{created!.Id}/special-dates", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
