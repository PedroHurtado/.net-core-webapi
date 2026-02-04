namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class RemoveServiceTests
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
    private readonly ServiceSchedule.RemoveService _removeService;
    private readonly Mock<RemoveService.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveService.Service _service;

    public RemoveServiceTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _removeService = new(_serviceScheduleValidator);
        _repositoryMock = new Mock<RemoveService.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveService.Service(
            _removeService,
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

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingService_RemovesService()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        await _service.HandleAsync(scheduleId, ServiceType.Lunch);

        existingSchedule.Services.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        await _service.HandleAsync(scheduleId, ServiceType.Lunch);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentScheduleId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveService.IService>();

        var result = await RemoveService.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
