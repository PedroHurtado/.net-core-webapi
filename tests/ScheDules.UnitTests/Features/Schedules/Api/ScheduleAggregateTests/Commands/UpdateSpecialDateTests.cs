namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class UpdateSpecialDateTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.AddSpecialDate _addSpecialDate;
    private readonly Schedule.UpdateSpecialDate _updateSpecialDate;
    private readonly Mock<UpdateSpecialDate.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateSpecialDate.Service _service;

    public UpdateSpecialDateTests()
    {
        _createSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _scheduleValidator);
        _updateSpecialDate = new(specialDateCreate, _scheduleValidator);

        _repositoryMock = new Mock<UpdateSpecialDate.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateSpecialDate.Service(_updateSpecialDate, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateScheduleWithSpecialDate(DateOnly date, bool isClosed = true, string reason = "Navidad")
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        var schedule = _createSchedule.Execute(command);

        var addCommand = new AddSpecialDateCommand(
            Date: date,
            IsClosed: isClosed,
            Reason: reason,
            TimeSlots: isClosed ? [] : [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        _addSpecialDate.Execute(schedule, addCommand);
        return schedule;
    }

    private static UpdateSpecialDate.Request CreateValidClosedRequest(string reason = "Navidad actualizado")
    {
        return new UpdateSpecialDate.Request(
            IsClosed: true,
            Reason: reason,
            TimeSlots: []);
    }

    private static UpdateSpecialDate.Request CreateValidOpenRequest(string reason = "Horario especial")
    {
        return new UpdateSpecialDate.Request(
            IsClosed: false,
            Reason: reason,
            TimeSlots: [new UpdateSpecialDate.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(18, 0))]);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_UpdatesReasonSuccessfully()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(reason: "Navidad (horario especial)");

        await _service.HandleAsync(scheduleId, date, request);

        schedule.SpecialDates.First().Reason.Should().Be("Navidad (horario especial)");
    }

    [Fact]
    public async Task HandleAsync_ChangesFromClosedToOpen()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date, isClosed: true);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidOpenRequest(reason: "Navidad con horario especial");

        await _service.HandleAsync(scheduleId, date, request);

        schedule.SpecialDates.First().IsClosed.Should().BeFalse();
        schedule.SpecialDates.First().TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_ChangesFromOpenToClosed()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 2, 14);
        var schedule = CreateScheduleWithSpecialDate(date, isClosed: false, reason: "San Valentín");
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(reason: "San Valentín cancelado");

        await _service.HandleAsync(scheduleId, date, request);

        schedule.SpecialDates.First().IsClosed.Should().BeTrue();
        schedule.SpecialDates.First().TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_UpdatesTimeSlots()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 2, 14);
        var schedule = CreateScheduleWithSpecialDate(date, isClosed: false, reason: "San Valentín");
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new UpdateSpecialDate.Request(
            IsClosed: false,
            Reason: "San Valentín",
            TimeSlots: [
                new UpdateSpecialDate.SetTimeSlotRequest(new TimeOnly(12, 0), new TimeOnly(16, 0)),
                new UpdateSpecialDate.SetTimeSlotRequest(new TimeOnly(19, 0), new TimeOnly(23, 59))
            ]);

        await _service.HandleAsync(scheduleId, date, request);

        schedule.SpecialDates.First().TimeSlots.Should().HaveCount(2);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        await _service.HandleAsync(scheduleId, date, request);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        await _service.HandleAsync(scheduleId, date, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Guard and Validation Tests

    [Fact]
    public async Task HandleAsync_WhenSpecialDateNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(new DateOnly(2025, 12, 31));
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        var act = () => _service.HandleAsync(scheduleId, date, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyReason_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(reason: "");

        var act = () => _service.HandleAsync(scheduleId, date, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Reason*required*");
    }

    [Fact]
    public async Task HandleAsync_WhenScheduleNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidClosedRequest();

        var act = () => _service.HandleAsync(scheduleId, date, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_OpenWithoutTimeSlots_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new UpdateSpecialDate.Request(
            IsClosed: false,
            Reason: "Test",
            TimeSlots: []);

        var act = () => _service.HandleAsync(scheduleId, date, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one time slot*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var request = CreateValidClosedRequest();
        var serviceMock = new Mock<UpdateSpecialDate.IService>();

        serviceMock.Setup(s => s.HandleAsync(scheduleId, date, request)).Returns(Task.CompletedTask);

        var result = await UpdateSpecialDate.Handler(serviceMock.Object, scheduleId, date, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var request = CreateValidClosedRequest();
        var serviceMock = new Mock<UpdateSpecialDate.IService>();

        await UpdateSpecialDate.Handler(serviceMock.Object, scheduleId, date, request);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, date, request), Times.Once);
    }

    #endregion
}
