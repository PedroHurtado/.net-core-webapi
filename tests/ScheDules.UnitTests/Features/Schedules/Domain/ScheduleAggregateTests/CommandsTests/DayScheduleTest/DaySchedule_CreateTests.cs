namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.DayScheduleTest;

public class DaySchedule_CreateTests
{
    private readonly DaySchedule.Create _command = new(
        new TimeSlot.Create(new TimeSlotValidator()),
        new DayScheduleValidator());

    [Fact]
    public void Execute_WithOpenDayAndSingleSlot_ReturnsDaySchedule()
    {
        var result = _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.IsClosed.Should().BeFalse();
        result.TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithOpenDayAndMultipleSlots_ReturnsDaySchedule()
    {
        var result = _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Tuesday,
            false,
            [
                new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new CreateTimeSlotCommand(new TimeOnly(20, 0), new TimeOnly(23, 0))
            ]));

        result.TimeSlots.Should().HaveCount(2);
        result.TotalOpenHours.Should().Be(TimeSpan.FromHours(6));
    }

    [Fact]
    public void Execute_WithClosedDay_ReturnsDaySchedule()
    {
        var result = _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Sunday,
            true,
            []));

        result.IsClosed.Should().BeTrue();
        result.TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithClosedDayAndTimeSlots_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Sunday,
            true,
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{DayScheduleValidationMessages.ClosedDayCannotHaveTimeSlots}*");
    }

    [Fact]
    public void Execute_WithOpenDayAndNoTimeSlots_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Monday,
            false,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{DayScheduleValidationMessages.OpenDayMustHaveTimeSlots}*");
    }

    [Fact]
    public void Execute_WithOverlappingTimeSlots_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateDayScheduleCommand(
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
    public void Execute_WithInvalidTimeSlot_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateDayScheduleCommand(
            DayOfWeek.Monday,
            false,
            [new CreateTimeSlotCommand(new TimeOnly(23, 0), new TimeOnly(13, 0))]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime}*");
    }
}
