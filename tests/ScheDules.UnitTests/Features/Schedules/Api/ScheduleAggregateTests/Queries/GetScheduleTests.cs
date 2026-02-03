namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Queries;

public class GetScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Mock<GetSchedule.IRepository> _repositoryMock;
    private readonly GetSchedule.Service _service;

    public GetScheduleTests()
    {
        _createSchedule = new(_scheduleValidator);
        _repositoryMock = new Mock<GetSchedule.IRepository>();
        _service = new GetSchedule.Service(_repositoryMock.Object);
    }

    private Schedule CreateExistingSchedule(
        string name = "Horario Test",
        string? description = null)
    {
        var command = new CreateScheduleCommand(_tenantId, name, description);
        return _createSchedule.Execute(command);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingSchedule_ReturnsResponse()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule(name: "Horario de Verano", description: "Descripción");
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var response = await _service.HandleAsync(scheduleId);

        response.Should().NotBeNull();
        response.Name.Should().Be("Horario de Verano");
        response.Description.Should().Be("Descripción");
    }

    [Fact]
    public async Task HandleAsync_MapsAllProperties()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var response = await _service.HandleAsync(scheduleId);

        response.Id.Should().Be(schedule.Id);
        response.Name.Should().Be(schedule.Name);
        response.Description.Should().Be(schedule.Description);
        response.IsActive.Should().Be(schedule.IsActive);
        response.HasWeeklyHours.Should().Be(schedule.HasWeeklyHours);
        response.HasSpecialDates.Should().Be(schedule.HasSpecialDates);
        response.IsFullyConfigured.Should().Be(schedule.IsFullyConfigured);
    }

    [Fact]
    public async Task HandleAsync_WithNullDescription_ReturnsNullDescription()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule(description: null);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var response = await _service.HandleAsync(scheduleId);

        response.Description.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    #endregion

    #region Exception Tests

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidId_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<GetSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await GetSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ScheduleResponse>>();
        var okResult = (Ok<ScheduleResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectId()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<GetSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await GetSchedule.Handler(serviceMock.Object, scheduleId);

        serviceMock.Verify(s => s.HandleAsync(scheduleId), Times.Once);
    }

    #endregion
}
