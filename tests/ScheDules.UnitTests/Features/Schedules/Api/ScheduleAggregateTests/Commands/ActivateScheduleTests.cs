namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class ActivateScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.Activate _activateSchedule;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Mock<ActivateSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ActivateSchedule.Service _service;

    public ActivateScheduleTests()
    {
        _createSchedule = new(_scheduleValidator);
        _activateSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _scheduleValidator);

        _repositoryMock = new Mock<ActivateSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ActivateSchedule.Service(_activateSchedule, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateScheduleWithWeeklyHours()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario con días", null);
        var schedule = _createSchedule.Execute(command);

        var setHoursCommand = new SetWeeklyHoursCommand(
            DayOfWeek: DayOfWeek.Monday,
            IsClosed: false,
            TimeSlots: [new CreateTimeSlotCommand(new TimeOnly(9, 0), new TimeOnly(17, 0))]);

        _setWeeklyHours.Execute(schedule, setHoursCommand);
        return schedule;
    }

    private Schedule CreateScheduleWithoutWeeklyHours()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario vacío", null);
        return _createSchedule.Execute(command);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidSchedule_ReturnsResponseWithIsActiveTrue()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        var response = await _service.HandleAsync(scheduleId);

        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCurrentActiveSchedule_DeactivatesPrevious()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();
        var currentActive = CreateScheduleWithWeeklyHours();

        var activateCommand = new ActivateScheduleCommand(null);
        _activateSchedule.Execute(currentActive, activateCommand);

        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync(currentActive);

        await _service.HandleAsync(scheduleId);

        currentActive.IsActive.Should().BeFalse();
        schedule.IsActive.Should().BeTrue();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        await _service.HandleAsync(scheduleId);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsFindFirstByIsActiveTrue()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        await _service.HandleAsync(scheduleId);

        _repositoryMock.Verify(r => r.FindFirstByIsActiveTrue(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        await _service.HandleAsync(scheduleId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Guard Tests

    [Fact]
    public async Task HandleAsync_WhenAlreadyActive_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithWeeklyHours();

        var activateCommand = new ActivateScheduleCommand(null);
        _activateSchedule.Execute(schedule, activateCommand);

        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public async Task HandleAsync_WithoutWeeklyHours_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithoutWeeklyHours();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((Schedule?)null);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one day configured*");
    }

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
    public async Task Handler_WithValidRequest_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<ActivateSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: true,
            HasWeeklyHours: true,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await ActivateSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ScheduleResponse>>();
        var okResult = (Ok<ScheduleResponse>)result;
        okResult.Value!.IsActive.Should().BeTrue();
    }

    #endregion
}
