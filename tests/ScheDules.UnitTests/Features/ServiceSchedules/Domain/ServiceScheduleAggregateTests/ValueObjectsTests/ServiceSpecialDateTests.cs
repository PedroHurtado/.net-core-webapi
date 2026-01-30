namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ServiceSpecialDateTests
{
    #region Properties

    [Fact]
    public void Date_SetsCorrectValue()
    {
        var date = new DateOnly(2025, 2, 14);
        var specialDate = new TestableServiceSpecialDate(date, true, new TimeOnly(19, 0), new TimeOnly(23, 0), null, "San Valentín");

        specialDate.Date.Should().Be(date);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailable_SetsCorrectValue(bool isAvailable)
    {
        var specialDate = new TestableServiceSpecialDate(new DateOnly(2025, 1, 1), isAvailable, null, null, null, "Año Nuevo");

        specialDate.IsAvailable.Should().Be(isAvailable);
    }

    [Fact]
    public void StartTime_SetsCorrectValue()
    {
        var startTime = new TimeOnly(19, 0);
        var specialDate = new TestableServiceSpecialDate(new DateOnly(2025, 2, 14), true, startTime, new TimeOnly(23, 0), null, null);

        specialDate.StartTime.Should().Be(startTime);
    }

    [Fact]
    public void EndTime_SetsCorrectValue()
    {
        var endTime = new TimeOnly(23, 0);
        var specialDate = new TestableServiceSpecialDate(new DateOnly(2025, 2, 14), true, new TimeOnly(19, 0), endTime, null, null);

        specialDate.EndTime.Should().Be(endTime);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(70)]
    [InlineData(null)]
    public void CapacityOverride_SetsCorrectValue(int? capacityOverride)
    {
        var specialDate = new TestableServiceSpecialDate(new DateOnly(2025, 12, 31), true, new TimeOnly(20, 0), new TimeOnly(23, 0), capacityOverride, null);

        specialDate.CapacityOverride.Should().Be(capacityOverride);
    }

    [Theory]
    [InlineData("San Valentín")]
    [InlineData("Año Nuevo")]
    [InlineData(null)]
    public void Reason_SetsCorrectValue(string? reason)
    {
        var specialDate = new TestableServiceSpecialDate(new DateOnly(2025, 1, 1), false, null, null, null, reason);

        specialDate.Reason.Should().Be(reason);
    }

    #endregion
}
