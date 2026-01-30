namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ServiceSpecialDateValidatorTests
{
    private readonly ServiceSpecialDateValidator _validator = new();

    [Fact]
    public void Validate_WithValidAvailableSpecialDate_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            "San Valentín");

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidUnavailableSpecialDate_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 1, 1),
            false,
            null,
            null,
            null,
            "Año Nuevo");

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidSpecialDateAndCapacityOverride_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 12, 31),
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 30),
            70,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #region Date Validation

    [Fact]
    public void Date_WhenEmpty_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            default,
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.DateRequired);
    }

    [Fact]
    public void Date_WhenValid_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region StartTime Validation

    [Fact]
    public void StartTime_WhenAvailableAndNull_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            null,
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.StartTimeRequired);
    }

    [Fact]
    public void StartTime_WhenNotAvailableAndHasValue_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 1, 1),
            false,
            new TimeOnly(19, 0),
            null,
            null,
            "Cerrado");

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.StartTimeMustBeEmpty);
    }

    [Fact]
    public void StartTime_WhenAvailableAndHasValue_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region EndTime Validation

    [Fact]
    public void EndTime_WhenAvailableAndNull_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            null,
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.EndTimeRequired);
    }

    [Fact]
    public void EndTime_WhenNotAvailableAndHasValue_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 1, 1),
            false,
            null,
            new TimeOnly(23, 0),
            null,
            "Cerrado");

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.EndTimeMustBeEmpty);
    }

    [Fact]
    public void EndTime_WhenBeforeStartTime_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(23, 0),
            new TimeOnly(19, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void EndTime_WhenEqualToStartTime_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(19, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void EndTime_WhenAfterStartTime_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CapacityOverride Validation

    [Fact]
    public void CapacityOverride_WhenZero_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            0,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.CapacityOverrideMustBeGreaterThanZero);
    }

    [Fact]
    public void CapacityOverride_WhenNegative_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            -10,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.CapacityOverrideMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void CapacityOverride_WhenGreaterThanZero_ReturnsSuccess(int capacityOverride)
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            capacityOverride,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CapacityOverride_WhenNull_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Reason Validation

    [Fact]
    public void Reason_WhenExceedsMaxLength_ReturnsError()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            new string('a', 201));

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceSpecialDateValidationMessages.ReasonMaxLength);
    }

    [Theory]
    [InlineData("San Valentín")]
    [InlineData("Año Nuevo - Cerrado")]
    public void Reason_WhenWithinMaxLength_ReturnsSuccess(string reason)
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            reason);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Reason_WhenNull_ReturnsSuccess()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            null);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
