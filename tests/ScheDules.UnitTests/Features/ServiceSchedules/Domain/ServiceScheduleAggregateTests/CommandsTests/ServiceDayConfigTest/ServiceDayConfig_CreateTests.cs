namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceDayConfigTest;

public class ServiceDayConfig_CreateTests
{
    private readonly ServiceDayConfig.Create _command = new(new ServiceDayConfigValidator());

    [Fact]
    public void Execute_WithValidAvailableConfig_ReturnsServiceDayConfig()
    {
        var result = _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0)));

        result.IsAvailable.Should().BeTrue();
        result.StartTime.Should().Be(new TimeOnly(13, 0));
        result.EndTime.Should().Be(new TimeOnly(16, 0));
        result.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Execute_WithCapacityOverride_ReturnsServiceDayConfig()
    {
        var result = _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 0),
            60));

        result.IsAvailable.Should().BeTrue();
        result.CapacityOverride.Should().Be(60);
    }

    [Fact]
    public void Execute_WithUnavailableConfig_ReturnsServiceDayConfig()
    {
        var result = _command.Execute(new CreateServiceDayConfigCommand(false));

        result.IsAvailable.Should().BeFalse();
        result.StartTime.Should().BeNull();
        result.EndTime.Should().BeNull();
        result.Duration.Should().BeNull();
    }

    [Fact]
    public void Execute_AvailableWithoutStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            true,
            null,
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public void Execute_AvailableWithoutEndTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(13, 0),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeRequired}*");
    }

    [Fact]
    public void Execute_EndTimeBeforeStartTime_OvernightRange_ReturnsServiceDayConfig()
    {
        var result = _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(20, 0),
            new TimeOnly(0, 30)));

        result.IsAvailable.Should().BeTrue();
        result.StartTime.Should().Be(new TimeOnly(20, 0));
        result.EndTime.Should().Be(new TimeOnly(0, 30));
        result.Duration.Should().Be(TimeSpan.FromHours(4) + TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void Execute_EndTimeEqualToStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(13, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeMustBeDifferentFromStartTime}*");
    }

    [Fact]
    public void Execute_UnavailableWithStartTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            false,
            new TimeOnly(13, 0),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.StartTimeMustBeEmpty}*");
    }

    [Fact]
    public void Execute_UnavailableWithEndTime_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            false,
            null,
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeMustBeEmpty}*");
    }

    [Fact]
    public void Execute_WithCapacityOverrideZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0),
            0));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithNegativeCapacityOverride_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceDayConfigCommand(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0),
            -1));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero}*");
    }
}
