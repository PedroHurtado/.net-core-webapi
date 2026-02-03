namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class AddSpecialDateTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.AddSpecialDate _addSpecialDate;
    private readonly Mock<AddSpecialDate.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddSpecialDate.Service _service;

    public AddSpecialDateTests()
    {
        _createSchedule = new(_scheduleValidator);

        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _scheduleValidator);

        _repositoryMock = new Mock<AddSpecialDate.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddSpecialDate.Service(_addSpecialDate, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateExistingSchedule()
    {
        var command = new CreateScheduleCommand(_tenantId, "Horario Test", null);
        return _createSchedule.Execute(command);
    }

    private static AddSpecialDate.Request CreateValidClosedRequest(
        DateOnly? date = null,
        string reason = "Navidad")
    {
        return new AddSpecialDate.Request(
            Date: date ?? new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: reason,
            TimeSlots: []);
    }

    private static AddSpecialDate.Request CreateValidOpenRequest(
        DateOnly? date = null,
        string reason = "San Valentín")
    {
        return new AddSpecialDate.Request(
            Date: date ?? new DateOnly(2025, 2, 14),
            IsClosed: false,
            Reason: reason,
            TimeSlots: [new AddSpecialDate.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(23, 0))]);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithClosedSpecialDate_AddsSpecialDate()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        var response = await _service.HandleAsync(scheduleId, request);

        response.HasSpecialDates.Should().BeTrue();
        response.SpecialDates.Should().HaveCount(1);
        response.SpecialDates.First().Date.Should().Be(new DateOnly(2025, 12, 25));
        response.SpecialDates.First().IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithOpenSpecialDate_AddsSpecialDateWithTimeSlots()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidOpenRequest();

        var response = await _service.HandleAsync(scheduleId, request);

        response.SpecialDates.First().IsClosed.Should().BeFalse();
        response.SpecialDates.First().TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleTimeSlots_AddsAllTimeSlots()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsClosed: false,
            Reason: "San Valentín",
            TimeSlots: [
                new AddSpecialDate.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new AddSpecialDate.SetTimeSlotRequest(new TimeOnly(20, 0), new TimeOnly(23, 59))
            ]);

        var response = await _service.HandleAsync(scheduleId, request);

        response.SpecialDates.First().TimeSlots.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_AddsMultipleSpecialDates()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request1 = CreateValidClosedRequest(new DateOnly(2025, 12, 25), "Navidad");
        var request2 = CreateValidClosedRequest(new DateOnly(2025, 12, 31), "Nochevieja");

        await _service.HandleAsync(scheduleId, request1);
        var response = await _service.HandleAsync(scheduleId, request2);

        response.SpecialDates.Should().HaveCount(2);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        await _service.HandleAsync(scheduleId, request);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest();

        await _service.HandleAsync(scheduleId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Guard and Validation Tests

    [Fact]
    public async Task HandleAsync_WithDuplicateDate_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(new DateOnly(2025, 12, 25));
        await _service.HandleAsync(scheduleId, request);

        var duplicateRequest = CreateValidClosedRequest(new DateOnly(2025, 12, 25), "Otro motivo");

        var act = () => _service.HandleAsync(scheduleId, duplicateRequest);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyReason_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(reason: "");

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Reason*required*");
    }

    [Fact]
    public async Task HandleAsync_WithReasonExceeding200Characters_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = CreateValidClosedRequest(reason: new string('a', 201));

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*200 characters*");
    }

    [Fact]
    public async Task HandleAsync_ClosedWithTimeSlots_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 12, 25),
            IsClosed: true,
            Reason: "Navidad",
            TimeSlots: [new AddSpecialDate.SetTimeSlotRequest(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*cannot have time slots*");
    }

    [Fact]
    public async Task HandleAsync_OpenWithoutTimeSlots_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(schedule);

        var request = new AddSpecialDate.Request(
            Date: new DateOnly(2025, 2, 14),
            IsClosed: false,
            Reason: "San Valentín",
            TimeSlots: []);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one time slot*");
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidClosedRequest();

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidClosedRequest();
        var serviceMock = new Mock<AddSpecialDate.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: true,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(scheduleId, request)).ReturnsAsync(expectedResponse);

        var result = await AddSpecialDate.Handler(serviceMock.Object, scheduleId, request);

        result.Should().BeOfType<Created<ScheduleResponse>>();
        var createdResult = (Created<ScheduleResponse>)result;
        createdResult.Location.Should().Be($"/schedules/{scheduleId}");
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidClosedRequest();
        var serviceMock = new Mock<AddSpecialDate.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: scheduleId,
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: true,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<AddSpecialDate.Request>()))
            .ReturnsAsync(expectedResponse);

        await AddSpecialDate.Handler(serviceMock.Object, scheduleId, request);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, request), Times.Once);
    }

    #endregion
}
