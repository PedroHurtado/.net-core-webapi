namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ReservationPolicyValidatorTests
{
    private readonly ReservationPolicyValidator _validator = new();

    #region Valid Policies

    [Fact]
    public void Validate_WithStandardValues_ReturnsSuccess()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithBufferBetweenReservations_ReturnsSuccess()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithStandardDurations_ReturnsSuccess()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);
        policy.StandardDurationsInternal[ServiceType.Breakfast] = TimeSpan.FromHours(1);
        policy.StandardDurationsInternal[ServiceType.Lunch] = TimeSpan.FromHours(1.5);
        policy.StandardDurationsInternal[ServiceType.Dinner] = TimeSpan.FromHours(2);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region MinimumAdvanceTime Validation

    [Fact]
    public void MinimumAdvanceTime_WhenZero_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.Zero,
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MinimumAdvanceTimeMustBeGreaterThanZero);
    }

    [Fact]
    public void MinimumAdvanceTime_WhenNegative_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(-1),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MinimumAdvanceTimeMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(24)]
    public void MinimumAdvanceTime_WhenGreaterThanZero_ReturnsSuccess(int hours)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(hours),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region MaximumAdvanceTime Validation

    [Fact]
    public void MaximumAdvanceTime_WhenLessThanMinimum_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(5),
            TimeSpan.FromHours(2),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum);
    }

    [Fact]
    public void MaximumAdvanceTime_WhenEqualToMinimum_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(2),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum);
    }

    [Theory]
    [InlineData(7, 7)]
    [InlineData(30, 30)]
    [InlineData(90, 90)]
    public void MaximumAdvanceTime_WhenGreaterThanMinimum_ReturnsSuccess(int minHours, int maxDays)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(minHours),
            TimeSpan.FromDays(maxDays),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region SlotInterval Validation

    [Fact]
    public void SlotInterval_WhenZero_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.Zero,
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.SlotIntervalMustBeGreaterThanZero);
    }

    [Fact]
    public void SlotInterval_WhenNegative_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(-15),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.SlotIntervalMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(45)]
    public void SlotInterval_WhenInvalidValue_ReturnsError(int minutes)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(minutes),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.SlotIntervalMustBeValid);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void SlotInterval_WhenValidValue_ReturnsSuccess(int minutes)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(minutes),
            TimeSpan.Zero,
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region BufferBetweenReservations Validation

    [Fact]
    public void BufferBetweenReservations_WhenNegative_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(-5),
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.BufferCannotBeNegative);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    public void BufferBetweenReservations_WhenZeroOrPositive_ReturnsSuccess(int minutes)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(minutes),
            8,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region PartySize Validation

    [Fact]
    public void MaxPartySize_WhenZero_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            0,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MaxPartySizeMustBeGreaterThanZero);
    }

    [Fact]
    public void MaxPartySize_WhenNegative_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            -5,
            1);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MaxPartySizeMustBeGreaterThanZero);
    }

    [Fact]
    public void MinPartySize_WhenZero_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            0);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MinPartySizeMustBeGreaterThanZero);
    }

    [Fact]
    public void MinPartySize_WhenNegative_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            -2);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MinPartySizeMustBeGreaterThanZero);
    }

    [Fact]
    public void MinPartySize_WhenGreaterThanMax_ReturnsError()
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            8,
            10);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ReservationPolicyValidationMessages.MinPartySizeCannotExceedMax);
    }

    [Theory]
    [InlineData(1, 8)]
    [InlineData(2, 10)]
    [InlineData(4, 4)]
    public void PartySize_WhenMinLessThanOrEqualToMax_ReturnsSuccess(int minPartySize, int maxPartySize)
    {
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero,
            maxPartySize,
            minPartySize);

        var result = _validator.Validate(policy);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
