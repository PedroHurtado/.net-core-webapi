namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_DeactivateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Schedule.Activate _activate;
    private readonly Schedule.Deactivate _deactivate;

    public Schedule_DeactivateTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _validator);
        _activate = new(_validator);
        _deactivate = new(_validator);
    }

    private Schedule CreateActiveSchedule()
    {
        var schedule = _create.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test Schedule",
            null));
        _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));
        _activate.Execute(schedule, new ActivateScheduleCommand(null));
        return schedule;
    }

    [Fact]
    public void Execute_WhenActive_DeactivatesSchedule()
    {
        var schedule = CreateActiveSchedule();

        var result = _deactivate.Execute(schedule);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var schedule = _create.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test Schedule",
            null));

        var act = () => _deactivate.Execute(schedule);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Schedule is already inactive*");
    }
}
