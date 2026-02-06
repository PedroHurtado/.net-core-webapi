namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class ConfigureServiceDayTests
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
    private readonly Mock<ConfigureServiceDay.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfigureServiceDay.Service _service;

    public ConfigureServiceDayTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _configureServiceDay = new(_createService, _serviceScheduleValidator);
        _repositoryMock = new Mock<ConfigureServiceDay.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ConfigureServiceDay.Service(
            _configureServiceDay,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static ConfigureServiceDay.Request CreateValidRequest(
        bool isAvailable = true,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        int? capacityOverride = null)
    {
        return new ConfigureServiceDay.Request(
            IsAvailable: isAvailable,
            StartTime: isAvailable ? (startTime ?? new TimeOnly(12, 0)) : null,
            EndTime: isAvailable ? (endTime ?? new TimeOnly(15, 0)) : null,
            CapacityOverride: capacityOverride);
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

    private ServiceSchedule CreateScheduleWithService(ServiceType type)
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            Type: type,
            MaxCapacity: 50,
            WeeklySchedule: new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
            {
                { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(15, 0)) }
            }));
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidData_ConfiguresDay()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(isAvailable: true, startTime: new TimeOnly(11, 0), endTime: new TimeOnly(14, 0));

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

        var service = existingSchedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.WeeklySchedule.Should().ContainKey(DayOfWeek.Friday);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

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

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Monday, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithAvailableDayMissingStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: null,
            EndTime: new TimeOnly(15, 0),
            CapacityOverride: null);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithAvailableDayMissingEndTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(12, 0),
            EndTime: null,
            CapacityOverride: null);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithEndTimeBeforeStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(13, 0),
            EndTime: new TimeOnly(13, 0),
            CapacityOverride: null);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeMustBeDifferentFromStartTime}*");
    }

    [Fact]
    public async Task HandleAsync_WithCapacityOverrideZero_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new ConfigureServiceDay.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(12, 0),
            EndTime: new TimeOnly(15, 0),
            CapacityOverride: 0);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DayOfWeek.Friday, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<ConfigureServiceDay.IService>();

        var result = await ConfigureServiceDay.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch, DayOfWeek.Monday, request);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
