namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class SpecialDateTests
{
    #region Constructor

    [Fact]
    public void Constructor_SetsDate()
    {
        var date = new DateOnly(2025, 12, 25);

        var specialDate = new TestableSpecialDate(date, true, "Navidad", []);

        specialDate.Date.Should().Be(date);
    }

    [Fact]
    public void Constructor_SetsIsClosed()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []);

        specialDate.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SetsReason()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []);

        specialDate.Reason.Should().Be("Navidad");
    }

    [Fact]
    public void Constructor_SetsTimeSlots()
    {
        var timeSlots = new[] { new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0)) };

        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            timeSlots);

        specialDate.TimeSlots.Should().BeEquivalentTo(timeSlots);
    }

    #endregion

    #region TotalOpenHours

    [Fact]
    public void TotalOpenHours_WhenClosed_ReturnsZero()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []);

        specialDate.TotalOpenHours.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void TotalOpenHours_WithTimeSlots_ReturnsSumOfDurations()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        specialDate.TotalOpenHours.Should().Be(TimeSpan.FromHours(10));
    }

    #endregion

    #region IsOpenAt

    [Fact]
    public void IsOpenAt_WhenClosedDate_ReturnsFalse()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []);

        specialDate.IsOpenAt(new TimeOnly(15, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_WhenTimeWithinSlot_ReturnsTrue()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        specialDate.IsOpenAt(new TimeOnly(18, 0)).Should().BeTrue();
    }

    [Fact]
    public void IsOpenAt_WhenTimeOutsideSlot_ReturnsFalse()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        specialDate.IsOpenAt(new TimeOnly(10, 0)).Should().BeFalse();
    }

    #endregion
}
