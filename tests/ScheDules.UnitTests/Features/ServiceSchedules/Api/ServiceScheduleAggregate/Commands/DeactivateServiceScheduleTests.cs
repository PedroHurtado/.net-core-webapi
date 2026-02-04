namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class DeactivateServiceScheduleTests
{
    private readonly ServiceScheduleValidator _serviceScheduleValidator = new();
    private readonly ServiceSchedule.Deactivate _deactivateServiceSchedule;
    private readonly Mock<DeactivateServiceSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateServiceSchedule.Service _service;

    public DeactivateServiceScheduleTests()
    {
        _deactivateServiceSchedule = new(_serviceScheduleValidator);
        _repositoryMock = new Mock<DeactivateServiceSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DeactivateServiceSchedule.Service(
            _deactivateServiceSchedule,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private ServiceSchedule CreateActiveSchedule()
    {
        var schedule = new ServiceSchedule(Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.TenantId))!
            .SetValue(schedule, Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Name))!
            .SetValue(schedule, "Test Schedule");
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Description))!
            .SetValue(schedule, "Test Description");
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.IsActive))!
            .SetValue(schedule, true);
        return schedule;
    }

    private ServiceSchedule CreateInactiveSchedule()
    {
        var schedule = CreateActiveSchedule();
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.IsActive))!
            .SetValue(schedule, false);
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithActiveSchedule_DeactivatesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var response = await _service.HandleAsync(scheduleId);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateActiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

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
    public async Task HandleAsync_WithAlreadyInactive_ThrowsConflictException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateInactiveSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var act = () => _service.HandleAsync(scheduleId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already inactive*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<DeactivateServiceSchedule.IService>();
        var expectedResponse = ServiceScheduleTestHelper.CreateMockResponse(scheduleId);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await DeactivateServiceSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ServiceScheduleResponse>>();
    }

    #endregion
}
