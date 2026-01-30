namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.SpecialDateTest;

public class SpecialDate_CreateTests
{
    private readonly SpecialDate.Create _command = new(
        new TimeSlot.Create(new TimeSlotValidator()),
        new SpecialDateValidator());

    [Fact]
    public void Execute_WithClosedHoliday_ReturnsSpecialDate()
    {
        var result = _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));

        result.Date.Should().Be(new DateOnly(2025, 12, 25));
        result.IsClosed.Should().BeTrue();
        result.Reason.Should().Be("Navidad");
        result.TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithSpecialHours_ReturnsSpecialDate()
    {
        var result = _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        result.IsClosed.Should().BeFalse();
        result.Reason.Should().Be("San Valentin");
        result.TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithVacationClosed_ReturnsSpecialDate()
    {
        var result = _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 8, 15),
            true,
            "Vacaciones de verano",
            []));

        result.IsClosed.Should().BeTrue();
        result.Reason.Should().Be("Vacaciones de verano");
    }

    [Fact]
    public void Execute_WithEmptyReason_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "",
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SpecialDateValidationMessages.ReasonRequired}*");
    }

    [Fact]
    public void Execute_WithReasonTooLong_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            new string('a', 201),
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SpecialDateValidationMessages.ReasonMaxLength}*");
    }

    [Fact]
    public void Execute_WithClosedDateAndTimeSlots_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SpecialDateValidationMessages.ClosedDateCannotHaveTimeSlots}*");
    }

    [Fact]
    public void Execute_WithOpenDateAndNoTimeSlots_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SpecialDateValidationMessages.OpenDateMustHaveTimeSlots}*");
    }

    [Fact]
    public void Execute_WithInvalidTimeSlot_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new CreateTimeSlotCommand(new TimeOnly(23, 0), new TimeOnly(13, 0))]));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime}*");
    }
}
