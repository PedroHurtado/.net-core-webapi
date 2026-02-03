namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_MarkDayAsClosedTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Schedule.MarkDayAsClosed _markDayAsClosed;

    public Schedule_MarkDayAsClosedTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _validator);
        _markDayAsClosed = new(dayScheduleCreate, _validator);
    }

    private Schedule CreateSchedule() => _create.Execute(new CreateScheduleCommand(
        Guid.NewGuid(),
        "Test Schedule",
        null));

    [Fact]
    public void Execute_WithNewDay_MarksDayAsClosed()
    {
        var schedule = CreateSchedule();

        var result = _markDayAsClosed.Execute(schedule, new MarkDayAsClosedCommand(DayOfWeek.Sunday));

        result.WeeklyHours.Should().ContainKey(DayOfWeek.Sunday);
        result.WeeklyHours[DayOfWeek.Sunday].IsClosed.Should().BeTrue();
        result.WeeklyHours[DayOfWeek.Sunday].TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithPreviouslyOpenDay_MarksDayAsClosed()
    {
        var schedule = CreateSchedule();
        _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        var result = _markDayAsClosed.Execute(schedule, new MarkDayAsClosedCommand(DayOfWeek.Monday));

        result.WeeklyHours[DayOfWeek.Monday].IsClosed.Should().BeTrue();
        result.WeeklyHours[DayOfWeek.Monday].TimeSlots.Should().BeEmpty();
    }
}
