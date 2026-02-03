namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_SetWeeklyHoursTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;

    public Schedule_SetWeeklyHoursTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _validator);
    }

    private Schedule CreateSchedule() => _create.Execute(new CreateScheduleCommand(
        Guid.NewGuid(),
        "Test Schedule",
        null));

    [Fact]
    public void Execute_WithSingleSlot_ConfiguresDay()
    {
        var schedule = CreateSchedule();

        var result = _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        result.WeeklyHours.Should().ContainKey(DayOfWeek.Monday);
        result.WeeklyHours[DayOfWeek.Monday].IsClosed.Should().BeFalse();
        result.WeeklyHours[DayOfWeek.Monday].TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleSlots_ConfiguresDay()
    {
        var schedule = CreateSchedule();

        var result = _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Tuesday,
            false,
            [
                new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new CreateTimeSlotCommand(new TimeOnly(20, 0), new TimeOnly(23, 0))
            ]));

        result.WeeklyHours[DayOfWeek.Tuesday].TimeSlots.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithExistingDay_OverwritesConfiguration()
    {
        var schedule = CreateSchedule();
        _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        var result = _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(12, 0), new TimeOnly(22, 0))]));

        result.WeeklyHours[DayOfWeek.Monday].TimeSlots.First().OpenTime.Should().Be(new TimeOnly(12, 0));
        result.WeeklyHours[DayOfWeek.Monday].TimeSlots.First().CloseTime.Should().Be(new TimeOnly(22, 0));
    }

    [Fact]
    public void Execute_WithOverlappingSlots_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [
                new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new CreateTimeSlotCommand(new TimeOnly(15, 0), new TimeOnly(18, 0))
            ]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{DayScheduleValidationMessages.TimeSlotsCannotOverlap}*");
    }

    [Fact]
    public void Execute_WithCloseTimeBeforeOpenTime_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(23, 0), new TimeOnly(13, 0))]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime}*");
    }
}
