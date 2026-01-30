namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class DayScheduleValidatorTests
{
    private readonly DayScheduleValidator _validator = new();

    [Fact]
    public void Validate_WithOpenDayAndSingleSlot_ReturnsSuccess()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOpenDayAndMultipleSlots_ReturnsSuccess()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Tuesday,
            false,
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new TestableTimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0))
            ]);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithClosedDay_ReturnsSuccess()
    {
        var daySchedule = new TestableDaySchedule(DayOfWeek.Sunday, true, []);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeTrue();
    }

    #region IsClosed Validation

    [Fact]
    public void TimeSlots_WhenClosedWithTimeSlots_ReturnsError()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Sunday,
            true,
            [new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DayScheduleValidationMessages.ClosedDayCannotHaveTimeSlots);
    }

    [Fact]
    public void TimeSlots_WhenOpenWithoutTimeSlots_ReturnsError()
    {
        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DayScheduleValidationMessages.OpenDayMustHaveTimeSlots);
    }

    #endregion

    #region Overlapping Validation

    [Fact]
    public void TimeSlots_WhenOverlapping_ReturnsError()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new TestableTimeSlot(new TimeOnly(15, 0), new TimeOnly(18, 0))
            ]);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == DayScheduleValidationMessages.TimeSlotsCannotOverlap);
    }

    [Fact]
    public void TimeSlots_WhenNotOverlapping_ReturnsSuccess()
    {
        var daySchedule = new TestableDaySchedule(
            DayOfWeek.Monday,
            false,
            [
                new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0)),
                new TestableTimeSlot(new TimeOnly(16, 0), new TimeOnly(19, 0))
            ]);

        var result = _validator.Validate(daySchedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
