namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Queries;

public class GetServiceSchedulesTests
{
    private readonly Mock<IQuery> _queryMock;
    private readonly GetServiceSchedules.Service _service;

    public GetServiceSchedulesTests()
    {
        _queryMock = new Mock<IQuery>();
        _service = new GetServiceSchedules.Service(_queryMock.Object);
    }

    private List<ServiceSchedule> CreateScheduleList(int activeCount, int inactiveCount)
    {
        var schedules = new List<ServiceSchedule>();

        for (int i = 0; i < activeCount; i++)
        {
            var schedule = new ServiceSchedule(Guid.NewGuid());
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.TenantId))!
                .SetValue(schedule, Guid.NewGuid());
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Name))!
                .SetValue(schedule, $"Active Schedule {i + 1}");
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Description))!
                .SetValue(schedule, "Description");
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.IsActive))!
                .SetValue(schedule, true);
            schedules.Add(schedule);
        }

        for (int i = 0; i < inactiveCount; i++)
        {
            var schedule = new ServiceSchedule(Guid.NewGuid());
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.TenantId))!
                .SetValue(schedule, Guid.NewGuid());
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Name))!
                .SetValue(schedule, $"Inactive Schedule {i + 1}");
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Description))!
                .SetValue(schedule, "Description");
            typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.IsActive))!
                .SetValue(schedule, false);
            schedules.Add(schedule);
        }

        return schedules;
    }

    private void SetupQueryMock(List<ServiceSchedule> schedules)
    {
        var queryable = schedules.AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<ServiceSchedule>()).Returns(queryable);
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithNoFilter_ReturnsAllSchedules()
    {
        var schedules = CreateScheduleList(activeCount: 2, inactiveCount: 3);
        SetupQueryMock(schedules);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(5);
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveTrue_ReturnsOnlyActiveSchedules()
    {
        var schedules = CreateScheduleList(activeCount: 2, inactiveCount: 3);
        SetupQueryMock(schedules);

        var response = await _service.HandleAsync(true);

        response.Should().HaveCount(2);
        response.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ReturnsOnlyInactiveSchedules()
    {
        var schedules = CreateScheduleList(activeCount: 2, inactiveCount: 3);
        SetupQueryMock(schedules);

        var response = await _service.HandleAsync(false);

        response.Should().HaveCount(3);
        response.Should().OnlyContain(s => !s.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WithNoSchedules_ReturnsEmptyList()
    {
        SetupQueryMock([]);

        var response = await _service.HandleAsync(null);

        response.Should().BeEmpty();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetServiceSchedules.IService>();
        var expectedResponse = new List<ServiceScheduleResponse>();

        serviceMock.Setup(s => s.HandleAsync(null)).ReturnsAsync(expectedResponse);

        var result = await GetServiceSchedules.Handler(serviceMock.Object, null);

        result.Should().BeOfType<Ok<List<ServiceScheduleResponse>>>();
    }

    #endregion
}
