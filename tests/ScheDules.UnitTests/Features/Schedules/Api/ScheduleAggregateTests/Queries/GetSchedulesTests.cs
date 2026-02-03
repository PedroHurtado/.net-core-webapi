namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleAggregateTests.Queries;

public class GetSchedulesTests
{
    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetSchedules.IService>();
        var expectedResponse = new List<ScheduleResponse>
        {
            new(
                Id: Guid.NewGuid(),
                Name: "Horario 1",
                Description: null,
                IsActive: true,
                HasWeeklyHours: true,
                HasSpecialDates: false,
                IsFullyConfigured: false,
                WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
                SpecialDates: []),
            new(
                Id: Guid.NewGuid(),
                Name: "Horario 2",
                Description: "Descripción",
                IsActive: false,
                HasWeeklyHours: false,
                HasSpecialDates: true,
                IsFullyConfigured: false,
                WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
                SpecialDates: [])
        };

        serviceMock.Setup(s => s.HandleAsync()).ReturnsAsync(expectedResponse);

        var result = await GetSchedules.Handler(serviceMock.Object);

        result.Should().BeOfType<Ok<List<ScheduleResponse>>>();
        var okResult = (Ok<List<ScheduleResponse>>)result;
        okResult.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_WithEmptyList_ReturnsOkWithEmptyArray()
    {
        var serviceMock = new Mock<GetSchedules.IService>();
        serviceMock.Setup(s => s.HandleAsync()).ReturnsAsync([]);

        var result = await GetSchedules.Handler(serviceMock.Object);

        result.Should().BeOfType<Ok<List<ScheduleResponse>>>();
        var okResult = (Ok<List<ScheduleResponse>>)result;
        okResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_CallsService()
    {
        var serviceMock = new Mock<GetSchedules.IService>();
        serviceMock.Setup(s => s.HandleAsync()).ReturnsAsync([]);

        await GetSchedules.Handler(serviceMock.Object);

        serviceMock.Verify(s => s.HandleAsync(), Times.Once);
    }

    #endregion
}
