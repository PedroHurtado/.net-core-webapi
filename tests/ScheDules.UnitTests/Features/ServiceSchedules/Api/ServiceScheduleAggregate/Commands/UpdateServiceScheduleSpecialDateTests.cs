namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceScheduleSpecialDateTests
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
    private readonly Service.UpdateSpecialDate _serviceUpdateSpecialDate;
    private readonly ReservationPolicy.Create _createReservationPolicy;
    private readonly ServiceSchedule.Create _createServiceSchedule;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;
    private readonly ServiceSchedule.UpdateSpecialDate _updateSpecialDate;
    private readonly Mock<UpdateServiceScheduleSpecialDate.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateServiceScheduleSpecialDate.Service _service;

    public UpdateServiceScheduleSpecialDateTests()
    {
        _createServiceDayConfig = new(_serviceDayConfigValidator);
        _createService = new(_createServiceDayConfig, _serviceValidator);
        _createServiceSpecialDate = new(_serviceSpecialDateValidator);
        _serviceAddSpecialDate = new(_serviceValidator);
        _serviceUpdateSpecialDate = new(_serviceValidator);
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _addService = new(_createService, _serviceScheduleValidator);
        _addSpecialDate = new(_createServiceSpecialDate, _serviceAddSpecialDate, _serviceScheduleValidator);
        _updateSpecialDate = new(_createServiceSpecialDate, _serviceUpdateSpecialDate, _serviceScheduleValidator);
        _repositoryMock = new Mock<UpdateServiceScheduleSpecialDate.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateServiceScheduleSpecialDate.Service(
            _updateSpecialDate,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static UpdateServiceScheduleSpecialDate.Request CreateValidRequest(
        bool isAvailable = true,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        int? capacityOverride = null,
        string? reason = null)
    {
        return new UpdateServiceScheduleSpecialDate.Request(
            IsAvailable: isAvailable,
            StartTime: isAvailable ? (startTime ?? new TimeOnly(11, 0)) : null,
            EndTime: isAvailable ? (endTime ?? new TimeOnly(16, 0)) : null,
            CapacityOverride: capacityOverride,
            Reason: reason);
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
    public async Task HandleAsync_WithValidData_UpdatesSpecialDate()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(startTime: new TimeOnly(11, 0), endTime: new TimeOnly(16, 0));

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        var service = existingSchedule.Services.First(s => s.Type == ServiceType.Lunch);
        var updatedSpecialDate = service.SpecialDates.First(sd => sd.Date == targetDate);
        updatedSpecialDate.StartTime.Should().Be(new TimeOnly(11, 0));
        updatedSpecialDate.EndTime.Should().Be(new TimeOnly(16, 0));
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

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

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today), request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today), request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDate_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateScheduleWithService(ServiceType.Lunch);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today.AddDays(99)), request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithAvailableDateMissingStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new UpdateServiceScheduleSpecialDate.Request(
            IsAvailable: true,
            StartTime: null,
            EndTime: new TimeOnly(15, 0),
            CapacityOverride: null,
            Reason: null);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithEndTimeBeforeStartTime_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = new UpdateServiceScheduleSpecialDate.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(15, 0),
            EndTime: new TimeOnly(12, 0),
            CapacityOverride: null,
            Reason: null);

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.EndTimeMustBeAfterStartTime}*");
    }

    [Fact]
    public async Task HandleAsync_WithReasonExceedingMaxLength_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(reason: new string('a', 201));

        var act = () => _service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.ReasonMaxLength}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateServiceScheduleSpecialDate.IService>();

        var result = await UpdateServiceScheduleSpecialDate.Handler(serviceMock.Object, scheduleId, ServiceType.Lunch, targetDate, request);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
