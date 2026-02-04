namespace Schedules.IntegrationTests.Schedules.Api.ScheduleAggregateTests.Commands;

public class CreateScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var request = new CreateSchedule.Request(
            Name: "Horario de Verano",
            Description: "Del 15 junio al 15 septiembre");

        var response = await Client.PostAsJsonAsync("/schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Horario de Verano");
        result.Description.Should().Be("Del 15 junio al 15 septiembre");
        result.IsActive.Should().BeFalse();
        result.HasWeeklyHours.Should().BeFalse();
        result.HasSpecialDates.Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns422()
    {
        var request = new CreateSchedule.Request(
            Name: "",
            Description: null);

        var response = await Client.PostAsJsonAsync("/schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithNameTooLong_Returns422()
    {
        var request = new CreateSchedule.Request(
            Name: new string('x', 101),
            Description: null);

        var response = await Client.PostAsJsonAsync("/schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithDescriptionTooLong_Returns422()
    {
        var request = new CreateSchedule.Request(
            Name: "Valid Name",
            Description: new string('x', 501));

        var response = await Client.PostAsJsonAsync("/schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
