namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class ActivateServiceScheduleTests
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
    private readonly ServiceSchedule.Activate _activateServiceSchedule;
    private readonly Mock<ActivateServiceSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ActivateServiceSchedule.Service _service;

    public ActivateServiceScheduleTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _activateServiceSchedule = new(_serviceScheduleValidator);
        _repositoryMock = new Mock<ActivateServiceSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ActivateServiceSchedule.Service(
            _activateServiceSchedule,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
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

    private ServiceSchedule CreateActiveScheduleWithService(ServiceType type)
    {
        var schedule = CreateScheduleWithService(type);
        _activateServiceSchedule.Execute(schedule, new ActivateServiceScheduleCommand(null));
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidData_ActivatesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((ServiceSchedule?)null);

        var response = await _service.HandleAsync(scheduleId);

        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCurrentActiveSchedule_DeactivatesPrevious()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        var currentActive = CreateActiveScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync(currentActive);

        await _service.HandleAsync(scheduleId);

        currentActive.IsActive.Should().BeFalse();
        existingSchedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((ServiceSchedule?)null);

        await _service.HandleAsync(scheduleId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WithAlreadyActive_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateActiveScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((ServiceSchedule?)null);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already active*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithNoServices_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        _repositoryMock.Setup(r => r.FindFirstByIsActiveTrue()).ReturnsAsync((ServiceSchedule?)null);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one service*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<ActivateServiceSchedule.IService>();
        var expectedResponse = ServiceScheduleTestHelper.CreateMockResponse(scheduleId);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await ActivateServiceSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ServiceScheduleResponse>>();
    }

    #endregion
}
