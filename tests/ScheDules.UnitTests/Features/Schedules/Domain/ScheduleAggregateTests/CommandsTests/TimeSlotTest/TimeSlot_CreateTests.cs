namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.TimeSlotTest;

public class TimeSlot_CreateTests
{
    private readonly TimeSlot.Create _command = new(new TimeSlotValidator());

    [Fact]
    public void Execute_WithValidTimeSlot_ReturnsTimeSlot()
    {
        var result = _command.Execute(new CreateTimeSlotCommand(
            new TimeOnly(13, 0),
            new TimeOnly(23, 0)));

        result.OpenTime.Should().Be(new TimeOnly(13, 0));
        result.CloseTime.Should().Be(new TimeOnly(23, 0));
        result.Duration.Should().Be(TimeSpan.FromHours(10));
    }

    [Fact]
    public void Execute_WithShortSlot_ReturnsTimeSlot()
    {
        var result = _command.Execute(new CreateTimeSlotCommand(
            new TimeOnly(13, 0),
            new TimeOnly(16, 0)));

        result.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Execute_WithFullDay_ReturnsTimeSlot()
    {
        var result = _command.Execute(new CreateTimeSlotCommand(
            new TimeOnly(0, 0),
            new TimeOnly(23, 59)));

        result.OpenTime.Should().Be(new TimeOnly(0, 0));
        result.CloseTime.Should().Be(new TimeOnly(23, 59));
    }

    [Fact]
    public void Execute_WithCloseTimeBeforeOpenTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateTimeSlotCommand(
            new TimeOnly(23, 0),
            new TimeOnly(13, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime}*");
    }

    [Fact]
    public void Execute_WithCloseTimeEqualToOpenTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateTimeSlotCommand(
            new TimeOnly(13, 0),
            new TimeOnly(13, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime}*");
    }
}
