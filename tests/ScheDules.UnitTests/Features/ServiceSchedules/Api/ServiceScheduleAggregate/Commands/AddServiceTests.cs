namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class AddServiceTests
{
    private readonly ServiceScheduleValidator _serviceScheduleValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceDayConfigValidator _serviceDayConfigValidator = new();
    private readonly ReservationPolicyValidator _reservationPolicyValidator = new();
    private readonly ServiceDayConfig.Create _createServiceDayConfig;
    private readonly Service.Create _createService;
    private readonly ReservationPolicy.Create _createReservationPolicy;
    private readonly ServiceSchedule.Create _createServiceSchedule;
    private readonly ServiceSchedule.AddService _addService;
    private readonly Mock<AddService.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddService.Service _service;

    public AddServiceTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _repositoryMock = new Mock<AddService.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddService.Service(
            _addService,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static AddService.Request CreateValidRequest(
        ServiceType type = ServiceType.Lunch,
        int maxCapacity = 50,
        Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>? weeklySchedule = null)
    {
        return new AddService.Request(
            Type: type,
            MaxCapacity: maxCapacity,
            WeeklySchedule: weeklySchedule ?? new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null) },
                { DayOfWeek.Tuesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null) },
                { DayOfWeek.Wednesday, new AddService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null) }
            });
    }

    private ServiceSchedule CreateSchedule()
    {
        return _createServiceSchedule.Execute(new CreateServiceScheduleCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Schedule",
            Description: "Test Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(15),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) }
            }));
    }

    private ServiceSchedule CreateScheduleWithService(ServiceType existingType)
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            Type: existingType,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
            {
                { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(15, 0)) }
            }));
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidData_AddsService()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(scheduleId, request);

        response.Services.Should().HaveCount(1);
        response.Services.First().Type.Should().Be(ServiceType.Lunch);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentScheduleId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WithDuplicateServiceType_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(type: ServiceType.Lunch);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithMaxCapacityZero_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(maxCapacity: 0);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public async Task HandleAsync_WithNoAvailableDays_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var weeklySchedule = new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
        {
            { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(false, null, null, null) },
            { DayOfWeek.Tuesday, new AddService.ServiceDayConfigInput(false, null, null, null) }
        };
        var request = CreateValidRequest(weeklySchedule: weeklySchedule);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }

    [Fact]
    public async Task HandleAsync_WithAvailableDayMissingStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var weeklySchedule = new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
        {
            { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, null, new TimeOnly(15, 0), null) }
        };
        var request = CreateValidRequest(weeklySchedule: weeklySchedule);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithAvailableDayMissingEndTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var weeklySchedule = new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
        {
            { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(12, 0), null, null) }
        };
        var request = CreateValidRequest(weeklySchedule: weeklySchedule);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithEndTimeBeforeStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var weeklySchedule = new Dictionary<DayOfWeek, AddService.ServiceDayConfigInput>
        {
            { DayOfWeek.Monday, new AddService.ServiceDayConfigInput(true, new TimeOnly(15, 0), new TimeOnly(12, 0), null) }
        };
        var request = CreateValidRequest(weeklySchedule: weeklySchedule);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeMustBeAfterStartTime}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddService.IService>();
        var expectedResponse = ServiceScheduleTestHelper.CreateMockResponse(scheduleId);

        serviceMock.Setup(s => s.HandleAsync(scheduleId, request)).ReturnsAsync(expectedResponse);

        var result = await AddService.Handler(serviceMock.Object, scheduleId, request);

        result.Should().BeOfType<Created<ServiceScheduleResponse>>();
    }

    #endregion
}
