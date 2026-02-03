namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class SetWeeklyHoursTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Mock<SetWeeklyHours.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetWeeklyHours.Service _service;

    public SetWeeklyHoursTests()
    {
        _createSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _scheduleValidator);

        _repositoryMock = new Mock<SetWeeklyHours.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SetWeeklyHours.Service(_setWeeklyHours, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateExistingSchedule()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        return _createSchedule.Execute(command);
    }

    private static SetWeeklyHours.Request CreateValidRequest(
        bool isClosed = false,
        params SetWeeklyHours.SetTimeSlotRequest[] timeSlots)
    {
        var slots = timeSlots.Length > 0
            ? timeSlots
            : [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(23, 0))];

        return new SetWeeklyHours.Request(IsClosed: isClosed, TimeSlots: slots);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ConfiguresDay()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        schedule.WeeklyHours.Should().ContainKey(DayOfWeek.Monday);
        schedule.WeeklyHours[DayOfWeek.Monday].IsClosed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleTimeSlots_ConfiguresAllSlots()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest(
            isClosed: false,
            new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(16, 0)),
            new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(20, 0), new TimeOnly(23, 0)));

        await _service.HandleAsync(scheduleId, DayOfWeek.Tuesday, request);

        schedule.WeeklyHours[DayOfWeek.Tuesday].TimeSlots.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WithClosedDay_ConfiguresClosedDay()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new SetWeeklyHours.Request(IsClosed: true, TimeSlots: []);

        await _service.HandleAsync(scheduleId, DayOfWeek.Sunday, request);

        schedule.WeeklyHours[DayOfWeek.Sunday].IsClosed.Should().BeTrue();
        schedule.WeeklyHours[DayOfWeek.Sunday].TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_OverwritesExistingConfiguration()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var firstRequest = CreateValidRequest();
        await _service.HandleAsync(scheduleId, DayOfWeek.Monday, firstRequest);

        var newRequest = new SetWeeklyHours.Request(
            IsClosed: false,
            TimeSlots: [new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(12, 0), new TimeOnly(22, 0))]);

        await _service.HandleAsync(scheduleId, DayOfWeek.Monday, newRequest);

        schedule.WeeklyHours[DayOfWeek.Monday].TimeSlots.Should().HaveCount(1);
        schedule.WeeklyHours[DayOfWeek.Monday].TimeSlots.First().OpenTime.Should().Be(new TimeOnly(12, 0));
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithOverlappingTimeSlots_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest(
            isClosed: false,
            new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(16, 0)),
            new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(15, 0), new TimeOnly(18, 0)));

        var act = () => _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*overlap*");
    }

    [Fact]
    public async Task HandleAsync_WithCloseTimeBeforeOpenTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidRequest(
            isClosed: false,
            new SetWeeklyHours.SetTimeSlotRequest(new TimeOnly(23, 0), new TimeOnly(13, 0)));

        var act = () => _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*after open time*");
    }

    [Fact]
    public async Task HandleAsync_WithOpenDayWithoutTimeSlots_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new SetWeeklyHours.Request(IsClosed: false, TimeSlots: []);

        var act = () => _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one time slot*");
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Monday;
        var request = CreateValidRequest();
        var serviceMock = new Mock<SetWeeklyHours.IService>();

        serviceMock.Setup(s => s.HandleAsync(scheduleId, dayOfWeek, request)).Returns(Task.CompletedTask);

        var result = await SetWeeklyHours.Handler(serviceMock.Object, scheduleId, dayOfWeek, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Wednesday;
        var request = CreateValidRequest();
        var serviceMock = new Mock<SetWeeklyHours.IService>();

        await SetWeeklyHours.Handler(serviceMock.Object, scheduleId, dayOfWeek, request);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, dayOfWeek, request), Times.Once);
    }

    #endregion
}
