namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class DeactivateScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.Activate _activateSchedule;
    private readonly Schedule.Deactivate _deactivateSchedule;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Mock<DeactivateSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateSchedule.Service _service;

    public DeactivateScheduleTests()
    {
        _createSchedule = new(_scheduleValidator);
        _activateSchedule = new(_scheduleValidator);
        _deactivateSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _scheduleValidator);

        _repositoryMock = new Mock<DeactivateSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DeactivateSchedule.Service(_deactivateSchedule, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateActiveSchedule()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Activo", null);
        var schedule = _createSchedule.Execute(command);

        var setHoursCommand = new SetWeeklyHoursCommand(
            DayOfWeek: DayOfWeek.Monday,
            IsClosed: false,
            TimeSlots: [new CreateTimeSlotCommand(new TimeOnly(9, 0), new TimeOnly(17, 0))]);

        _setWeeklyHours.Execute(schedule, setHoursCommand);

        var activateCommand = new ActivateScheduleCommand(null);
        _activateSchedule.Execute(schedule, activateCommand);

        return schedule;
    }

    private Schedule CreateInactiveSchedule()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Inactivo", null);
        return _createSchedule.Execute(command);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithActiveSchedule_ReturnsResponseWithIsActiveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var response = await _service.HandleAsync(scheduleId);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithActiveSchedule_DeactivatesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId);

        schedule.IsActive.Should().BeFalse();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Guard Tests

    [Fact]
    public async Task HandleAsync_WhenAlreadyInactive_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateInactiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenGuardFails_DoesNotCallUnitOfWork()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateInactiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        try { await _service.HandleAsync(scheduleId); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<DeactivateSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: true,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await DeactivateSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ScheduleResponse>>();
        var okResult = (Ok<ScheduleResponse>)result;
        okResult.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectId()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<DeactivateSchedule.IService>();
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

        await DeactivateSchedule.Handler(serviceMock.Object, scheduleId);

        serviceMock.Verify(s => s.HandleAsync(scheduleId), Times.Once);
    }

    #endregion
}
