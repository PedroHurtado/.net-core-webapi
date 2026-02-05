namespace Schedules.IntegrationTests.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class CreateServiceScheduleTests(WebApplicationFactory<Program> factory) : SchedulesWebApplicationFixture(factory)
{
    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "Horario Verano",
            Description: "Junio a Septiembre",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Breakfast, TimeSpan.FromHours(1) },
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
                { ServiceType.Dinner, TimeSpan.FromHours(2) }
            });

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<ServiceScheduleResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Horario Verano");
        result.Description.Should().Be("Junio a Septiembre");
        result.IsActive.Should().BeFalse();
        result.HasServices.Should().BeFalse();
        result.ServiceCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "",
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithNameTooLong_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: new string('x', 101),
            Description: "Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithEmptyDescription_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "Valid Name",
            Description: "",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithDescriptionTooLong_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "Valid Name",
            Description: new string('x', 501),
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithInvalidSlotInterval_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "Valid Name",
            Description: "Valid Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(7),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithMinAdvanceGreaterThanMaxAdvance_Returns422()
    {
        var request = new CreateServiceSchedule.Request(
            Name: "Valid Name",
            Description: "Valid Description",
            MinimumAdvanceTime: TimeSpan.FromHours(5),
            MaximumAdvanceTime: TimeSpan.FromHours(2),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: []);

        var response = await Client.PostAsJsonAsync("/service-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
