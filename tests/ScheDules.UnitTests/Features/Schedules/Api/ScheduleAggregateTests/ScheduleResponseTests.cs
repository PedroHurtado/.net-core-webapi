namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleAggregateTests;

public class ScheduleResponseTests
{
    #region ScheduleResponse.Map

    [Fact]
    public void ScheduleResponse_Map_MapsAllProperties()
    {
        var scheduleId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var schedule = new TestableSchedule(scheduleId);
        schedule.SetTenantId(tenantId);
        schedule.SetName("Business Hours");
        schedule.SetDescription("Regular business schedule");
        schedule.SetIsActive(true);

        var response = ScheduleResponse.Map(schedule);

        response.Id.Should().Be(scheduleId);
        response.Name.Should().Be("Business Hours");
        response.Description.Should().Be("Regular business schedule");
        response.IsActive.Should().BeTrue();
        response.HasWeeklyHours.Should().BeFalse();
        response.HasSpecialDates.Should().BeFalse();
        response.IsFullyConfigured.Should().BeFalse();
        response.WeeklyHours.Should().BeEmpty();
        response.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void ScheduleResponse_Map_WithNullDescription_MapsNullValue()
    {
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Simple Schedule");

        var response = ScheduleResponse.Map(schedule);

        response.Description.Should().BeNull();
    }

    [Fact]
    public void ScheduleResponse_Map_WithWeeklyHours_MapsWeeklyHours()
    {
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");

        var timeSlots = new List<TimeSlot>
        {
            new TestableTimeSlot(new TimeOnly(9, 0), new TimeOnly(17, 0))
        }.AsReadOnly();

        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, timeSlots);
        schedule.AddWeeklyHours(DayOfWeek.Monday, daySchedule);

        var response = ScheduleResponse.Map(schedule);

        response.HasWeeklyHours.Should().BeTrue();
        response.WeeklyHours.Should().HaveCount(1);
        response.WeeklyHours.Should().ContainKey(DayOfWeek.Monday);
    }

    [Fact]
    public void ScheduleResponse_Map_WithSpecialDates_MapsSpecialDates()
    {
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");

        var specialDate = new TestableSpecialDate(
            new DateOnly(2024, 12, 25),
            true,
            "Christmas",
            new List<TimeSlot>().AsReadOnly());
        schedule.SetSpecialDate(specialDate);

        var response = ScheduleResponse.Map(schedule);

        response.HasSpecialDates.Should().BeTrue();
        response.SpecialDates.Should().HaveCount(1);
        response.SpecialDates.First().Date.Should().Be(new DateOnly(2024, 12, 25));
        response.SpecialDates.First().Reason.Should().Be("Christmas");
    }

    [Fact]
    public void ScheduleResponse_Map_FullyConfigured_ReturnsTrue()
    {
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Full Schedule");

        var timeSlots = new List<TimeSlot>
        {
            new TestableTimeSlot(new TimeOnly(9, 0), new TimeOnly(17, 0))
        }.AsReadOnly();

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var daySchedule = new TestableDaySchedule(day, false, timeSlots);
            schedule.AddWeeklyHours(day, daySchedule);
        }

        var response = ScheduleResponse.Map(schedule);

        response.IsFullyConfigured.Should().BeTrue();
        response.WeeklyHours.Should().HaveCount(7);
    }

    #endregion

    #region DayScheduleResponse.Map

    [Fact]
    public void DayScheduleResponse_Map_MapsAllProperties()
    {
        var timeSlots = new List<TimeSlot>
        {
            new TestableTimeSlot(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            new TestableTimeSlot(new TimeOnly(14, 0), new TimeOnly(18, 0))
        }.AsReadOnly();

        var daySchedule = new TestableDaySchedule(DayOfWeek.Tuesday, false, timeSlots);

        var response = DayScheduleResponse.Map(daySchedule);

        response.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        response.IsClosed.Should().BeFalse();
        response.TimeSlots.Should().HaveCount(2);
        response.TotalOpenHours.Should().Be(TimeSpan.FromHours(7));
    }

    [Fact]
    public void DayScheduleResponse_Map_ClosedDay_MapsCorrectly()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Sunday,
            true,
            new List<TimeSlot>().AsReadOnly());

        var response = DayScheduleResponse.Map(daySchedule);

        response.DayOfWeek.Should().Be(DayOfWeek.Sunday);
        response.IsClosed.Should().BeTrue();
        response.TimeSlots.Should().BeEmpty();
        response.TotalOpenHours.Should().Be(TimeSpan.Zero);
    }

    #endregion

    #region SpecialDateResponse.Map

    [Fact]
    public void SpecialDateResponse_Map_MapsAllProperties()
    {
        var timeSlots = new List<TimeSlot>
        {
            new TestableTimeSlot(new TimeOnly(10, 0), new TimeOnly(14, 0))
        }.AsReadOnly();

        var specialDate = new TestableSpecialDate(
            new DateOnly(2024, 1, 1),
            false,
            "New Year's Day - Reduced Hours",
            timeSlots);

        var response = SpecialDateResponse.Map(specialDate);

        response.Date.Should().Be(new DateOnly(2024, 1, 1));
        response.IsClosed.Should().BeFalse();
        response.Reason.Should().Be("New Year's Day - Reduced Hours");
        response.TimeSlots.Should().HaveCount(1);
        response.TotalOpenHours.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void SpecialDateResponse_Map_ClosedDate_MapsCorrectly()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2024, 12, 25),
            true,
            "Christmas Day",
            new List<TimeSlot>().AsReadOnly());

        var response = SpecialDateResponse.Map(specialDate);

        response.Date.Should().Be(new DateOnly(2024, 12, 25));
        response.IsClosed.Should().BeTrue();
        response.Reason.Should().Be("Christmas Day");
        response.TimeSlots.Should().BeEmpty();
        response.TotalOpenHours.Should().Be(TimeSpan.Zero);
    }

    #endregion

    #region TimeSlotResponse.Map

    [Fact]
    public void TimeSlotResponse_Map_MapsAllProperties()
    {
        var timeSlot = new TestableTimeSlot(
            new TimeOnly(9, 30),
            new TimeOnly(17, 45));

        var response = TimeSlotResponse.Map(timeSlot);

        response.OpenTime.Should().Be(new TimeOnly(9, 30));
        response.CloseTime.Should().Be(new TimeOnly(17, 45));
        response.Duration.Should().Be(TimeSpan.FromHours(8) + TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void TimeSlotResponse_Map_FullDay_MapsCorrectly()
    {
        var timeSlot = new TestableTimeSlot(
            new TimeOnly(0, 0),
            new TimeOnly(23, 59));

        var response = TimeSlotResponse.Map(timeSlot);

        response.OpenTime.Should().Be(new TimeOnly(0, 0));
        response.CloseTime.Should().Be(new TimeOnly(23, 59));
        response.Duration.Should().Be(TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59));
    }

    #endregion
}
