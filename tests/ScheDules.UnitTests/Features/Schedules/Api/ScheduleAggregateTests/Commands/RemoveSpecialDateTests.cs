namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class RemoveSpecialDateTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.AddSpecialDate _addSpecialDate;
    private readonly Schedule.RemoveSpecialDate _removeSpecialDate;
    private readonly Mock<RemoveSpecialDate.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveSpecialDate.Service _service;

    public RemoveSpecialDateTests()
    {
        _createSchedule = new(_scheduleValidator);
        _removeSpecialDate = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _scheduleValidator);

        _repositoryMock = new Mock<RemoveSpecialDate.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveSpecialDate.Service(_removeSpecialDate, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateScheduleWithSpecialDate(DateOnly date, string reason = "Navidad")
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        var schedule = _createSchedule.Execute(command);

        var addCommand = new AddSpecialDateCommand(
            Date: date,
            IsClosed: true,
            Reason: reason,
            TimeSlots: []);

        _addSpecialDate.Execute(schedule, addCommand);
        return schedule;
    }

    private Schedule CreateScheduleWithMultipleSpecialDates()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        var schedule = _createSchedule.Execute(command);

        var dates = new[]
        {
            (new DateOnly(2025, 12, 25), "Navidad"),
            (new DateOnly(2025, 12, 31), "Nochevieja"),
            (new DateOnly(2025, 1, 1), "Año Nuevo")
        };

        foreach (var (date, reason) in dates)
        {
            var addCommand = new AddSpecialDateCommand(
                Date: date,
                IsClosed: true,
                Reason: reason,
                TimeSlots: []);

            _addSpecialDate.Execute(schedule, addCommand);
        }

        return schedule;
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_RemovesSpecialDateSuccessfully()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        schedule.SpecialDates.Should().HaveCount(1);

        await _service.HandleAsync(scheduleId, date);

        schedule.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RemovesOnlySpecifiedDate()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithMultipleSpecialDates();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        schedule.SpecialDates.Should().HaveCount(3);

        await _service.HandleAsync(scheduleId, new DateOnly(2025, 12, 25));

        schedule.SpecialDates.Should().HaveCount(2);
        schedule.SpecialDates.Should().NotContain(sd => sd.Date == new DateOnly(2025, 12, 25));
        schedule.SpecialDates.Should().Contain(sd => sd.Date == new DateOnly(2025, 12, 31));
        schedule.SpecialDates.Should().Contain(sd => sd.Date == new DateOnly(2025, 1, 1));
    }

    [Fact]
    public async Task HandleAsync_RemovesLastSpecialDate_LeavesEmptyCollection()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId, date);

        schedule.SpecialDates.Should().BeEmpty();
        schedule.HasSpecialDates.Should().BeFalse();
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

        await _service.HandleAsync(scheduleId, date);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var schedule = CreateScheduleWithSpecialDate(date);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        await _service.HandleAsync(scheduleId, date);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Guard Tests

    [Fact]
    public async Task HandleAsync_WhenSpecialDateNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithSpecialDate(new DateOnly(2025, 12, 31));
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var act = () => _service.HandleAsync(scheduleId, new DateOnly(2025, 8, 15));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenScheduleNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId, new DateOnly(2025, 12, 25));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenGuardFails_DoesNotCallUnitOfWork()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateScheduleWithSpecialDate(new DateOnly(2025, 12, 31));
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        try { await _service.HandleAsync(scheduleId, new DateOnly(2025, 8, 15)); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var serviceMock = new Mock<RemoveSpecialDate.IService>();

        serviceMock.Setup(s => s.HandleAsync(scheduleId, date)).Returns(Task.CompletedTask);

        var result = await RemoveSpecialDate.Handler(serviceMock.Object, scheduleId, date);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var date = new DateOnly(2025, 12, 25);
        var serviceMock = new Mock<RemoveSpecialDate.IService>();

        await RemoveSpecialDate.Handler(serviceMock.Object, scheduleId, date);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, date), Times.Once);
    }

    #endregion
}
