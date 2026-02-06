namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceScheduleSpecialDateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ServiceSchedule.Create _createServiceSchedule = fixture.Get<ServiceSchedule.Create>();
    private readonly ServiceSchedule.AddService _addService = fixture.Get<ServiceSchedule.AddService>();
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate = fixture.Get<ServiceSchedule.AddSpecialDate>();
    private readonly ServiceSchedule.UpdateSpecialDate _updateSpecialDate = fixture.Get<ServiceSchedule.UpdateSpecialDate>();
    private readonly Mock<UpdateServiceScheduleSpecialDate.IRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UpdateServiceScheduleSpecialDate.Service CreateService() =>
        new(_updateSpecialDate, _repositoryMock.Object, _unitOfWorkMock.Object);

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
        var service = CreateService();

        await service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        var svc = existingSchedule.Services.First(s => s.Type == ServiceType.Lunch);
        var updatedSpecialDate = svc.SpecialDates.First(sd => sd.Date == targetDate);
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
        var service = CreateService();

        await service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

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
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today), request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentServiceType_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today), request);

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
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, DateOnly.FromDateTime(DateTime.Today.AddDays(99)), request);

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
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

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
            StartTime: new TimeOnly(13, 0),
            EndTime: new TimeOnly(13, 0),
            CapacityOverride: null,
            Reason: null);
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.EndTimeMustBeDifferentFromStartTime}*");
    }

    [Fact]
    public async Task HandleAsync_WithReasonExceedingMaxLength_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var existingSchedule = CreateScheduleWithServiceAndSpecialDate(ServiceType.Lunch, targetDate);
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(reason: new string('a', 201));
        var service = CreateService();

        var act = () => service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

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