namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ServiceDayConfigValidatorTests
{
    private readonly ServiceDayConfigValidator _validator = new();

    [Fact]
    public void Validate_WithValidAvailableConfig_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidUnavailableConfig_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(false, null, null, null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidConfigAndCapacityOverride_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), 60);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #region StartTime Validation

    [Fact]
    public void StartTime_WhenAvailableAndNull_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, null, new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.StartTimeRequired);
    }

    [Fact]
    public void StartTime_WhenNotAvailableAndHasValue_ReturnsError()
    {
        var config = new TestableServiceDayConfig(false, new TimeOnly(13, 0), null, null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.StartTimeMustBeEmpty);
    }

    [Fact]
    public void StartTime_WhenAvailableAndHasValue_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region EndTime Validation

    [Fact]
    public void EndTime_WhenAvailableAndNull_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), null, null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.EndTimeRequired);
    }

    [Fact]
    public void EndTime_WhenNotAvailableAndHasValue_ReturnsError()
    {
        var config = new TestableServiceDayConfig(false, null, new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.EndTimeMustBeEmpty);
    }

    [Fact]
    public void EndTime_WhenBeforeStartTime_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(16, 0), new TimeOnly(13, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void EndTime_WhenEqualToStartTime_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(13, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void EndTime_WhenAfterStartTime_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CapacityOverride Validation

    [Fact]
    public void CapacityOverride_WhenZero_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), 0);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero);
    }

    [Fact]
    public void CapacityOverride_WhenNegative_ReturnsError()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), -5);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceDayConfigValidationMessages.CapacityOverrideMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void CapacityOverride_WhenGreaterThanZero_ReturnsSuccess(int capacityOverride)
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), capacityOverride);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CapacityOverride_WhenNull_ReturnsSuccess()
    {
        var config = new TestableServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
