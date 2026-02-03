namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class MarkDayAsClosedTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.MarkDayAsClosed _markDayAsClosed;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Mock<MarkDayAsClosed.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkDayAsClosed.Service _service;

    public MarkDayAsClosedTests()
    {
        _createSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _markDayAsClosed = new(dayScheduleCreate, _scheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _scheduleValidator);

        _repositoryMock = new Mock<MarkDayAsClosed.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new MarkDayAsClosed.Service(_markDayAsClosed, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateExistingSchedule()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        return _createSchedule.Execute(command);
    }

    private Schedule CreateScheduleWithOpenDay(DayOfWeek day)
    {
        var schedule = CreateExistingSchedule();

        var setHoursCommand = new SetWeeklyHoursCommand(
            DayOfWeek: day,
            IsClosed: false,
            TimeSlots: [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        _setWeeklyHours.Execute(schedule, setHoursCommand);
        return schedule;
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_MarksNewDayAsClosed()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId, DayOfWeek.Sunday);

        schedule.WeeklyHours.Should().ContainKey(DayOfWeek.Sunday);
        schedule.WeeklyHours[DayOfWeek.Sunday].IsClosed.Should().BeTrue();
        schedule.WeeklyHours[DayOfWeek.Sunday].TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_OverwritesPreviouslyOpenDay()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithOpenDay(DayOfWeek.Monday);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        schedule.WeeklyHours[DayOfWeek.Monday].IsClosed.Should().BeFalse();

        await _service.HandleAsync(scheduleId, DayOfWeek.Monday);

        schedule.WeeklyHours[DayOfWeek.Monday].IsClosed.Should().BeTrue();
        schedule.WeeklyHours[DayOfWeek.Monday].TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_AllDaysOfWeek_CanBeMarkedAsClosed()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            await _service.HandleAsync(scheduleId, day);
        }

        schedule.WeeklyHours.Should().HaveCount(7);
        schedule.WeeklyHours.Values.Should().OnlyContain(ds => ds.IsClosed);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId, DayOfWeek.Sunday);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId, DayOfWeek.Sunday);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Exception Tests

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId, DayOfWeek.Sunday);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Sunday;
        var serviceMock = new Mock<MarkDayAsClosed.IService>();

        serviceMock.Setup(s => s.HandleAsync(scheduleId, dayOfWeek)).Returns(Task.CompletedTask);

        var result = await MarkDayAsClosed.Handler(serviceMock.Object, scheduleId, dayOfWeek);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Friday;
        var serviceMock = new Mock<MarkDayAsClosed.IService>();

        await MarkDayAsClosed.Handler(serviceMock.Object, scheduleId, dayOfWeek);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, dayOfWeek), Times.Once);
    }

    #endregion
}
