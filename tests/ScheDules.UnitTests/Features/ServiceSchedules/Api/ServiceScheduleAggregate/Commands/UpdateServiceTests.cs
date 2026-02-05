namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceTests
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
    private readonly ServiceSchedule.UpdateService _updateService;
    private readonly Mock<UpdateService.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateService.Service _service;

    public UpdateServiceTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _updateService = new(_createService, _serviceScheduleValidator);
        _repositoryMock = new Mock<UpdateService.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateService.Service(
            _updateService,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static UpdateService.Request CreateValidRequest(
        int maxCapacity = 100,
        Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>? weeklySchedule = null)
    {
        return new UpdateService.Request(
            MaxCapacity: maxCapacity,
            WeeklySchedule: weeklySchedule ?? new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
            {
                { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(16, 0), null) },
                { DayOfWeek.Tuesday, new UpdateService.ServiceDayConfigInput(true, new TimeOnly(12, 0), new TimeOnly(16, 0), null) }
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
    public async Task HandleAsync_WithValidData_UpdatesService()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(maxCapacity: 100);

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

        var updatedService = existingSchedule.Services.First(s => s.Type == ServiceType.Lunch);
        updatedService.MaxCapacity.Should().Be(100);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

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

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithMaxCapacityZero_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(maxCapacity: 0);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public async Task HandleAsync_WithNoAvailableDays_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var weeklySchedule = new Dictionary<DayOfWeek, UpdateService.ServiceDayConfigInput>
        {
            { DayOfWeek.Monday, new UpdateService.ServiceDayConfigInput(false, null, null, null) }
        };
        var request = CreateValidRequest(weeklySchedule: weeklySchedule);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateService.IService>();

        var result = await UpdateService.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch, request);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
