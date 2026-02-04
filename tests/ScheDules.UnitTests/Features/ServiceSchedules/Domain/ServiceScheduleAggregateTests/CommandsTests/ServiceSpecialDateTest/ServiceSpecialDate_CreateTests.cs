namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceSpecialDateTest;

public class ServiceSpecialDate_CreateTests
{
    private readonly ServiceSpecialDate.Create _command = new(new ServiceSpecialDateValidator());

    [Fact]
    public void Execute_WithValidAvailableSpecialDate_ReturnsServiceSpecialDate()
    {
        var result = _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 30),
            null,
            "San Valentín"));

        result.Date.Should().Be(new DateOnly(2025, 2, 14));
        result.IsAvailable.Should().BeTrue();
        result.StartTime.Should().Be(new TimeOnly(19, 0));
        result.EndTime.Should().Be(new TimeOnly(23, 30));
        result.Reason.Should().Be("San Valentín");
    }

    [Fact]
    public void Execute_WithUnavailableSpecialDate_ReturnsServiceSpecialDate()
    {
        var result = _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 1, 1),
            false,
            null,
            null,
            null,
            "Año Nuevo"));

        result.Date.Should().Be(new DateOnly(2025, 1, 1));
        result.IsAvailable.Should().BeFalse();
        result.StartTime.Should().BeNull();
        result.EndTime.Should().BeNull();
        result.Reason.Should().Be("Año Nuevo");
    }

    [Fact]
    public void Execute_WithCapacityOverride_ReturnsServiceSpecialDate()
    {
        var result = _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 12, 31),
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 59),
            70,
            "Nochevieja"));

        result.CapacityOverride.Should().Be(70);
    }

    [Fact]
    public void Execute_WithEmptyDate_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            default,
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.DateRequired}*");
    }

    [Fact]
    public void Execute_WithReasonExceedingMaxLength_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 8, 15),
            false,
            null,
            null,
            null,
            new string('a', 201)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.ReasonMaxLength}*");
    }

    [Fact]
    public void Execute_AvailableWithoutStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            null,
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public void Execute_AvailableWithoutEndTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(13, 0),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.EndTimeRequired}*");
    }

    [Fact]
    public void Execute_EndTimeBeforeStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(16, 0),
            new TimeOnly(13, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.EndTimeMustBeAfterStartTime}*");
    }

    [Fact]
    public void Execute_UnavailableWithStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 1, 1),
            false,
            new TimeOnly(13, 0),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.StartTimeMustBeEmpty}*");
    }

    [Fact]
    public void Execute_UnavailableWithEndTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 1, 1),
            false,
            null,
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.EndTimeMustBeEmpty}*");
    }

    [Fact]
    public void Execute_WithCapacityOverrideZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0),
            0));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.CapacityOverrideMustBeGreaterThanZero}*");
    }
}
