namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class GetServiceScheduleTests
{
    private readonly Mock<GetServiceSchedule.IRepository> _repositoryMock;
    private readonly GetServiceSchedule.Service _service;

    public GetServiceScheduleTests()
    {
        _repositoryMock = new Mock<GetServiceSchedule.IRepository>();
        _service = new GetServiceSchedule.Service(_repositoryMock.Object);
    }

    private ServiceSchedule CreateSchedule()
    {
        var schedule = new ServiceSchedule(Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.TenantId))!
            .SetValue(schedule, Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Name))!
            .SetValue(schedule, "Test Schedule");
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Description))!
            .SetValue(schedule, "Test Description");
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.IsActive))!
            .SetValue(schedule, false);
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsServiceScheduleResponse()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var response = await _service.HandleAsync(scheduleId);

        response.Should().NotBeNull();
        response.Name.Should().Be("Test Schedule");
        response.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        await _service.HandleAsync(scheduleId);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
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

    #region Handler Tests

    [Fact]
    public async Task Handler_WithExistingId_ReturnsOkResult()
    {
        var scheduleId = Guid.NewGuid();
        var serviceMock = new Mock<GetServiceSchedule.IService>();
        var expectedResponse = ServiceScheduleTestHelper.CreateMockResponse(scheduleId);

        serviceMock.Setup(s => s.HandleAsync(scheduleId)).ReturnsAsync(expectedResponse);

        var result = await GetServiceSchedule.Handler(serviceMock.Object, scheduleId);

        result.Should().BeOfType<Ok<ServiceScheduleResponse>>();
    }

    #endregion
}
