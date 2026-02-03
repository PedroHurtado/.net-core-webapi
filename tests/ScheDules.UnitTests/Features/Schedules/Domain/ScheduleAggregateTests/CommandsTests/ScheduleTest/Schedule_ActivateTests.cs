namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_ActivateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly DayScheduleValidator _dayScheduleValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.SetWeeklyHours _setWeeklyHours;
    private readonly Schedule.Activate _activate;

    public Schedule_ActivateTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var dayScheduleCreate = new DaySchedule.Create(timeSlotCreate, _dayScheduleValidator);
        _setWeeklyHours = new(dayScheduleCreate, _validator);
        _activate = new(_validator);
    }

    private Schedule CreateSchedule() => _create.Execute(new CreateScheduleCommand(
        Guid.NewGuid(),
        "Test Schedule",
        null));

    private Schedule CreateScheduleWithWeeklyHours()
    {
        var schedule = CreateSchedule();
        _setWeeklyHours.Execute(schedule, new SetWeeklyHoursCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));
        return schedule;
    }

    [Fact]
    public void Execute_WithWeeklyHoursConfigured_ActivatesSchedule()
    {
        var schedule = CreateScheduleWithWeeklyHours();

        var result = _activate.Execute(schedule, new ActivateScheduleCommand(null));

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithCurrentActive_DeactivatesPrevious()
    {
        var currentActive = CreateScheduleWithWeeklyHours();
        _activate.Execute(currentActive, new ActivateScheduleCommand(null));
        var newSchedule = CreateScheduleWithWeeklyHours();

        var result = _activate.Execute(newSchedule, new ActivateScheduleCommand(currentActive));

        result.IsActive.Should().BeTrue();
        currentActive.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var schedule = CreateScheduleWithWeeklyHours();
        _activate.Execute(schedule, new ActivateScheduleCommand(null));

        var act = () => _activate.Execute(schedule, new ActivateScheduleCommand(null));

        act.Should().Throw<ConflictException>()
            .WithMessage("*Schedule is already active*");
    }

    [Fact]
    public void Execute_WithoutWeeklyHours_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _activate.Execute(schedule, new ActivateScheduleCommand(null));

        act.Should().Throw<ValidationException>()
            .WithMessage("*Schedule must have at least one day configured*");
    }
}
