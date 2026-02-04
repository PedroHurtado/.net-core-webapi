namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ReservationPolicyTest;

public class ReservationPolicy_CreateTests
{
    private readonly ReservationPolicy.Create _command = new(new ReservationPolicyValidator());

    [Fact]
    public void Execute_WithValidStandardValues_ReturnsReservationPolicy()
    {
        var result = _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        result.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(2));
        result.MaximumAdvanceTime.Should().Be(TimeSpan.FromDays(30));
        result.SlotInterval.Should().Be(TimeSpan.FromMinutes(15));
        result.MaxPartySize.Should().Be(8);
        result.MinPartySize.Should().Be(1);
        result.SlotIntervalMinutes.Should().Be(15);
        result.MaxAdvanceDays.Should().Be(30);
    }

    [Fact]
    public void Execute_WithBufferBetweenReservations_ReturnsReservationPolicy()
    {
        var result = _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            8,
            1,
            []));

        result.BufferBetweenReservations.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void Execute_WithStandardDurations_ReturnsReservationPolicy()
    {
        var durations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        var result = _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            durations));

        result.StandardDurations.Should().HaveCount(3);
        result.GetDurationFor(ServiceType.Breakfast).Should().Be(TimeSpan.FromHours(1));
        result.GetDurationFor(ServiceType.Lunch).Should().Be(TimeSpan.FromMinutes(90));
        result.GetDurationFor(ServiceType.Dinner).Should().Be(TimeSpan.FromHours(2));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void Execute_WithValidSlotInterval_ReturnsReservationPolicy(int minutes)
    {
        var result = _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(minutes),
            TimeSpan.Zero,
            8,
            1,
            []));

        result.SlotInterval.Should().Be(TimeSpan.FromMinutes(minutes));
    }

    [Fact]
    public void Execute_WithMinimumAdvanceTimeZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.Zero,
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinimumAdvanceTimeMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithMaxAdvanceLessThanMinAdvance_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(5),
            TimeSpan.FromHours(2),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum}*");
    }

    [Theory]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(45)]
    [InlineData(90)]
    public void Execute_WithInvalidSlotInterval_ThrowsValidationException(int minutes)
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(minutes),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.SlotIntervalMustBeValid}*");
    }

    [Fact]
    public void Execute_WithSlotIntervalZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.Zero,
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.SlotIntervalMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithMinPartySizeGreaterThanMaxPartySize_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            10,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinPartySizeCannotExceedMax}*");
    }

    [Fact]
    public void Execute_WithNegativeBuffer_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(-5),
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.BufferCannotBeNegative}*");
    }

    [Fact]
    public void Execute_WithMaxPartySizeZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            0,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MaxPartySizeMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithMinPartySizeZero_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateReservationPolicyCommand(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            0,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinPartySizeMustBeGreaterThanZero}*");
    }
}
