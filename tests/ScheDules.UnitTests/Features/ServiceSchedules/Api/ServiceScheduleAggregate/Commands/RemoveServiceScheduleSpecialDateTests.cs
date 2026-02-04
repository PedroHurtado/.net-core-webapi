namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class RemoveServiceScheduleSpecialDateTests
{
    private readonly ServiceScheduleValidator _serviceScheduleValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceDayConfigValidator _serviceDayConfigValidator = new();
    private readonly ServiceSpecialDateValidator _serviceSpecialDateValidator = new();
    private readonly ReservationPolicyValidator _reservationPolicyValidator = new();
    private readonly ServiceDayConfig.Create _createServiceDayConfig;
    private readonly Service.Create _createService;
    private readonly ServiceSpecialDate.Create _createServiceSpecialDate;
    private readonly Service.AddSpecialDate _serviceAddSpecialDate;
    private readonly Service.RemoveSpecialDate _serviceRemoveSpecialDate;
    private readonly ReservationPolicy.Create _createReservationPolicy;
    private readonly ServiceSchedule.Create _createServiceSchedule;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;
    private readonly ServiceSchedule.RemoveSpecialDate _removeSpecialDate;
    private readonly Mock<RemoveServiceScheduleSpecialDate.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveServiceScheduleSpecialDate.Service _service;

    public RemoveServiceScheduleSpecialDateTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createServiceSpecialDate = new(_serviceSpecialDateValidator);
        _serviceAddSpecialDate = new(_serviceValidator);
        _serviceRemoveSpecialDate = new(_serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _addSpecialDate = new(_createServiceSpecialDate, _serviceAddSpecialDate, _serviceScheduleValidator);
        _removeSpecialDate = new(_serviceRemoveSpecialDate, _serviceScheduleValidator);
        _repositoryMock = new Mock<RemoveServiceScheduleSpecialDate.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveServiceScheduleSpecialDate.Service(
            _removeSpecialDate,
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

    private ServiceSchedule CreateScheduleWithServiceAndSpecialDate(ServiceType type, DateOnly existingDate)
    {
        var schedule = CreateScheduleWithService(type);
        _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            Type: type,
            Date: existingDate,
            IsAvailable: true,
            StartTime: new TimeOnly(10, 0),
            EndTime: new TimeOnly(14, 0),
            CapacityOverride: null,
            Reason: null));
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingSpecialDate_RemovesSpecialDate()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate);

        var service = existingSchedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentScheduleId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDate_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today.AddDays(99)));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var serviceMock = new Mock<RemoveServiceScheduleSpecialDate.IService>();

        var result = await RemoveServiceScheduleSpecialDate.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch, targetDate);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
