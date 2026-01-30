namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.ValueObjectsTests;

public class TimeSlotValidatorTests
{
    private readonly TimeSlotValidator _validator = new();

    [Fact]
    public void Validate_WithValidTimeSlot_ReturnsSuccess()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithShortValidTimeSlot_ReturnsSuccess()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithFullDayTimeSlot_ReturnsSuccess()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(0, 0), new TimeOnly(23, 59));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeTrue();
    }

    #region CloseTime Validation

    [Fact]
    public void CloseTime_WhenBeforeOpenTime_ReturnsError()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(23, 0), new TimeOnly(13, 0));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime);
    }

    [Fact]
    public void CloseTime_WhenEqualToOpenTime_ReturnsError()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(13, 0));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TimeSlotValidationMessages.CloseTimeMustBeAfterOpenTime);
    }

    [Fact]
    public void CloseTime_WhenAfterOpenTime_ReturnsSuccess()
    {
        var timeSlot = new TestableTimeSlot(new TimeOnly(13, 0), new TimeOnly(14, 0));

        var result = _validator.Validate(timeSlot);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
