namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class CanReserveTests
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
    private readonly ServiceSchedule.ConfigureServiceDay _configureServiceDay;
    private readonly ServiceSchedule.Activate _activateServiceSchedule;
    private readonly Mock<CanReserve.IRepository> _repositoryMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly CanReserve.Service _service;

    public CanReserveTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _configureServiceDay = new(_createService, _serviceScheduleValidator);
        _activateServiceSchedule = new(_serviceScheduleValidator);
        _repositoryMock = new Mock<CanReserve.IRepository>();
        _timeProviderMock = new Mock<TimeProvider>();
        _service = new CanReserve.Service(_repositoryMock.Object, _timeProviderMock.Object);
    }

    private ServiceSchedule CreateSchedule()
    {
        return _createServiceSchedule.Execute(new CreateServiceScheduleCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Schedule",
            Description: "Test Description",
            MinimumAdvanceTime: TimeSpan.FromHours(2),
            MaximumAdvanceTime: TimeSpan.FromDays(30),
            SlotInterval: TimeSpan.FromMinutes(30),
            BufferBetweenReservations: TimeSpan.FromMinutes(15),
            MaxPartySize: 8,
            MinPartySize: 1,
            StandardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
                { ServiceType.Dinner, TimeSpan.FromMinutes(120) }
            }));
    }

    private ServiceSchedule CreateFullyConfiguredSchedule(ServiceType serviceType)
    {
        var schedule = CreateSchedule();

        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>();
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            weeklySchedule[day] = new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0));
        }

        _addService.Execute(schedule, new AddServiceCommand(
            Type: serviceType,
            MaxCapacity: 50,
            WeeklySchedule: weeklySchedule));

        _activateServiceSchedule.Execute(schedule, new ActivateServiceScheduleCommand(null));

        return schedule;
    }

    private ServiceSchedule CreateScheduleWithMondayOnlyUnavailable(ServiceType serviceType)
    {
        var schedule = CreateSchedule();

        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(false, null, null) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) },
            { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) },
            { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) },
            { DayOfWeek.Saturday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) },
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(22, 0)) }
        };

        _addService.Execute(schedule, new AddServiceCommand(
            Type: serviceType,
            MaxCapacity: 50,
            WeeklySchedule: weeklySchedule));

        _activateServiceSchedule.Execute(schedule, new ActivateServiceScheduleCommand(null));

        return schedule;
    }

    private void SetupCurrentTime(DateTime now)
    {
        _timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(new DateTimeOffset(now));
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidReservation_ReturnsCanReserveTrue()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 10, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = now.AddHours(4);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 4);

        response.CanReserve.Should().BeTrue();
        response.Reason.Should().BeNull();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentScheduleId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateTime.Now.AddDays(1), 4);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Business Logic Tests - CanReserve False

    [Fact]
    public async Task HandleAsync_WithUnavailableService_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithMondayOnlyUnavailable(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 2, 10, 0, 0);
        SetupCurrentTime(now);

        var monday = new DateTime(2025, 6, 2, 13, 0, 0);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, monday, 4);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("not available");
    }

    [Fact]
    public async Task HandleAsync_WithTimeOutsideServiceHours_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 8, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = now.AddHours(3);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 4);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("only available from");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidAdvanceTime_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 12, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = now.AddMinutes(30);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 4);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("at least");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPartySize_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 10, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = now.AddHours(4);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 15);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("Maximum");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlot_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 10, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = new DateTime(2025, 6, 1, 14, 17, 0);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 4);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("Reservations only accepted");
    }

    [Fact]
    public async Task HandleAsync_WithNotEnoughTimeBeforeClose_ReturnsCanReserveFalse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateFullyConfiguredSchedule(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var now = new DateTime(2025, 6, 1, 10, 0, 0);
        SetupCurrentTime(now);

        var reservationTime = new DateTime(2025, 6, 1, 21, 30, 0);

        var response = await _service.HandleAsync(scheduleId, ServiceType.Lunch, reservationTime, 4);

        response.CanReserve.Should().BeFalse();
        response.Reason.Should().Contain("Not enough time");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<CanReserve.IService>();
        var expectedResponse = new CanReserve.Response(true, null);

        serviceMock.Setup(s => s.HandleAsync(scheduleId, ServiceType.Lunch, It.IsAny<DateTime>(), 4))
            .ReturnsAsync(expectedResponse);

        var result = await CanReserve.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch, DateTime.Now.AddDays(1), 4);

        result.Should().BeOfType<Ok<CanReserve.Response>>();
    }

    #endregion
}
