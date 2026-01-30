namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ServiceDayConfigTests
{
    #region Properties

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailable_SetsCorrectValue(bool isAvailable)
    {
        var config = new TestableServiceDayConfig(isAvailable, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        config.IsAvailable.Should().Be(isAvailable);
    }

    [Fact]
    public void StartTime_SetsCorrectValue()
    {
        var startTime = new TimeOnly(13, 0);
        var config = new TestableServiceDayConfig(true, startTime, new TimeOnly(16, 0), null);

        config.StartTime.Should().Be(startTime);
    }

    [Fact]
    public void EndTime_SetsCorrectValue()
    {
        var endTime = new TimeOnly(16, 0);
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), endTime, null);

        config.EndTime.Should().Be(endTime);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(null)]
    public void CapacityOverride_SetsCorrectValue(int? capacityOverride)
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), capacityOverride);

        config.CapacityOverride.Should().Be(capacityOverride);
    }

    #endregion

    #region Duration

    [Fact]
    public void Duration_WhenAvailable_ReturnsCorrectTimeSpan()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        config.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Duration_WhenAvailableWithLongService_ReturnsCorrectTimeSpan()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null);

        config.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Duration_WhenNotAvailable_ReturnsNull()
    {
        var config = new TestableServiceDayConfig(false, null, null, null);

        config.Duration.Should().BeNull();
    }

    #endregion

    #region Static Instances

    [Fact]
    public void Unavailable_ReturnsCorrectServiceDayConfig()
    {
        var instance = ServiceDayConfig.Unavailable();

        instance.IsAvailable.Should().BeFalse();
        instance.StartTime.Should().BeNull();
        instance.EndTime.Should().BeNull();
        instance.CapacityOverride.Should().BeNull();
    }

    #endregion
}
