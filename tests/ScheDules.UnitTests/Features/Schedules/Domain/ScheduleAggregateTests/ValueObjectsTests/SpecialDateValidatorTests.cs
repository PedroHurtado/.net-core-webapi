namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class SpecialDateValidatorTests
{
    private readonly SpecialDateValidator _validator = new();

    [Fact]
    public void Validate_WithClosedHoliday_ReturnsSuccess()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSpecialHours_ReturnsSuccess()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithVacationClosed_ReturnsSuccess()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 8, 15),
            true,
            "Vacaciones de verano",
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #region Reason Validation

    [Fact]
    public void Reason_WhenEmpty_ReturnsError()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "",
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SpecialDateValidationMessages.ReasonRequired);
    }

    [Fact]
    public void Reason_WhenTooLong_ReturnsError()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            new string('a', 201),
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SpecialDateValidationMessages.ReasonMaxLength);
    }

    [Fact]
    public void Reason_WhenMaxLength_ReturnsSuccess()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            new string('a', 200),
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region TimeSlots Validation

    [Fact]
    public void TimeSlots_WhenClosedWithTimeSlots_ReturnsError()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SpecialDateValidationMessages.ClosedDateCannotHaveTimeSlots);
    }

    [Fact]
    public void TimeSlots_WhenOpenWithoutTimeSlots_ReturnsError()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            []);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SpecialDateValidationMessages.OpenDateMustHaveTimeSlots);
    }

    [Fact]
    public void TimeSlots_WhenOverlapping_ReturnsError()
    {
        var specialDate = new TestableSpecialDate(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentin",
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(18, 0)),
                new TestableTimeSlot(new TimeOnly(17, 0), new TimeOnly(23, 0))
            ]);

        var result = _validator.Validate(specialDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SpecialDateValidationMessages.TimeSlotsCannotOverlap);
    }

    #endregion
}
