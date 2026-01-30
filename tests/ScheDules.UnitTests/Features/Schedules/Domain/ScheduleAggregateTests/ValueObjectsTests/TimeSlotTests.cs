namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class TimeSlotTests
{
    #region Constructor

    [Fact]
    public void Constructor_SetsOpenTime()
    {
        var openTime = new TimeOnly(13, 0);
        var closeTime = new TimeOnly(23, 0);

        var timeSlot = new TestableTimeSlot(openTime, closeTime);

        timeSlot.OpenTime.Should().Be(openTime);
    }

    [Fact]
    public void Constructor_SetsCloseTime()
    {
        var openTime = new TimeOnly(13, 0);
        var closeTime = new TimeOnly(23, 0);

        var timeSlot = new TestableTimeSlot(openTime, closeTime);

        timeSlot.CloseTime.Should().Be(closeTime);
    }

    #endregion

    #region Duration

    [Fact]
    public void Duration_ReturnsCorrectTimeSpan()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Duration.Should().Be(TimeSpan.FromHours(10));
    }

    [Fact]
    public void Duration_WithShortSlot_ReturnsCorrectTimeSpan()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));

        timeSlot.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Duration_WithFullDay_ReturnsCorrectTimeSpan()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(0, 0), new TimeOnly(23, 59));

        timeSlot.Duration.Should().Be(new TimeSpan(23, 59, 0));
    }

    #endregion

    #region Contains

    [Fact]
    public void Contains_WhenTimeWithinRange_ReturnsTrue()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Contains(new TimeOnly(15, 0)).Should().BeTrue();
    }

    [Fact]
    public void Contains_WhenTimeOutsideRange_ReturnsFalse()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Contains(new TimeOnly(11, 0)).Should().BeFalse();
    }

    [Fact]
    public void Contains_WhenTimeAtOpenTime_ReturnsTrue()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Contains(new TimeOnly(13, 0)).Should().BeTrue();
    }

    [Fact]
    public void Contains_WhenTimeAtCloseTime_ReturnsTrue()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Contains(new TimeOnly(23, 0)).Should().BeTrue();
    }

    [Fact]
    public void Contains_WhenTimeAfterCloseTime_ReturnsFalse()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        timeSlot.Contains(new TimeOnly(23, 30)).Should().BeFalse();
    }

    #endregion

    #region OverlapsWith

    [Fact]
    public void OverlapsWith_WhenSlotsOverlap_ReturnsTrue()
    {
        var slot1 = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TestableTimeSlot(new TimeOnly(15, 0), new TimeOnly(18, 0));

        slot1.OverlapsWith(slot2).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenSlotsDoNotOverlap_ReturnsFalse()
    {
        var slot1 = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TestableTimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0));

        slot1.OverlapsWith(slot2).Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_WhenSlotContainsOther_ReturnsTrue()
    {
        var slot1 = new TestableTimeSlot(new TimeOnly(10, 0), new TimeOnly(20, 0));
        var slot2 = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));

        slot1.OverlapsWith(slot2).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenSlotsAreAdjacent_ReturnsFalse()
    {
        var slot1 = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TestableTimeSlot(new TimeOnly(16, 0), new TimeOnly(19, 0));

        slot1.OverlapsWith(slot2).Should().BeFalse();
    }

    #endregion
}
