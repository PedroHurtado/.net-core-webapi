namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class DayScheduleTests
{
    #region Constructor

    [Fact]
    public void Constructor_SetsDayOfWeek()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        daySchedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void Constructor_SetsIsClosed()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Sunday,
            true,
            []);

        daySchedule.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SetsTimeSlots()
    {
        var timeSlots = new[] { new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0)) };

        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, timeSlots);

        daySchedule.TimeSlots.Should().BeEquivalentTo(timeSlots);
    }

    #endregion

    #region TotalOpenHours

    [Fact]
    public void TotalOpenHours_WithSingleSlot_ReturnsCorrectDuration()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        daySchedule.TotalOpenHours.Should().Be(TimeSpan.FromHours(10));
    }

    [Fact]
    public void TotalOpenHours_WithMultipleSlots_ReturnsSumOfDurations()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Tuesday,
            false,
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new TestableTimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0))
            ]);

        daySchedule.TotalOpenHours.Should().Be(TimeSpan.FromHours(6));
    }

    [Fact]
    public void TotalOpenHours_WhenClosed_ReturnsZero()
    {
        var daySchedule = new TestableDaySchedule(DayOfWeek.Sunday, true, []);

        daySchedule.TotalOpenHours.Should().Be(TimeSpan.Zero);
    }

    #endregion

    #region IsOpenAt

    [Fact]
    public void IsOpenAt_WhenTimeWithinSlot_ReturnsTrue()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        daySchedule.IsOpenAt(new TimeOnly(15, 0)).Should().BeTrue();
    }

    [Fact]
    public void IsOpenAt_WhenTimeOutsideSlot_ReturnsFalse()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        daySchedule.IsOpenAt(new TimeOnly(11, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_WhenTimeBetweenSlots_ReturnsFalse()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new TestableTimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0))
            ]);

        daySchedule.IsOpenAt(new TimeOnly(18, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_WhenClosed_ReturnsFalse()
    {
        var daySchedule = new TestableDaySchedule(DayOfWeek.Sunday, true, []);

        daySchedule.IsOpenAt(new TimeOnly(15, 0)).Should().BeFalse();
    }

    #endregion
}
