namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.ValueObjectsTests;

public class ReservationPolicyTests
{
    #region Properties

    [Fact]
    public void MinimumAdvanceTime_SetsCorrectValue()
    {
        var minAdvance = TimeSpan.FromHours(2);
        var policy = new TestableReservationPolicy(minAdvance, TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        policy.MinimumAdvanceTime.Should().Be(minAdvance);
    }

    [Fact]
    public void MaximumAdvanceTime_SetsCorrectValue()
    {
        var maxAdvance = TimeSpan.FromDays(30);
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), maxAdvance, TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        policy.MaximumAdvanceTime.Should().Be(maxAdvance);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void SlotInterval_SetsCorrectValue(int minutes)
    {
        var slotInterval = TimeSpan.FromMinutes(minutes);
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), slotInterval, TimeSpan.Zero, 8, 1);

        policy.SlotInterval.Should().Be(slotInterval);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    public void BufferBetweenReservations_SetsCorrectValue(int minutes)
    {
        var buffer = TimeSpan.FromMinutes(minutes);
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), buffer, 8, 1);

        policy.BufferBetweenReservations.Should().Be(buffer);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(12)]
    public void MaxPartySize_SetsCorrectValue(int maxPartySize)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, maxPartySize, 1);

        policy.MaxPartySize.Should().Be(maxPartySize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void MinPartySize_SetsCorrectValue(int minPartySize)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, minPartySize);

        policy.MinPartySize.Should().Be(minPartySize);
    }

    #endregion

    #region Collections

    [Fact]
    public void StandardDurations_InitiallyEmpty()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        policy.StandardDurations.Should().BeEmpty();
    }

    [Fact]
    public void StandardDurations_CanAddDurations()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);
        policy.StandardDurationsInternal[ServiceType.Breakfast] = TimeSpan.FromHours(1);
        policy.StandardDurationsInternal[ServiceType.Lunch] = TimeSpan.FromHours(1.5);
        policy.StandardDurationsInternal[ServiceType.Dinner] = TimeSpan.FromHours(2);

        policy.StandardDurations.Should().HaveCount(3);
        policy.StandardDurations[ServiceType.Breakfast].Should().Be(TimeSpan.FromHours(1));
        policy.StandardDurations[ServiceType.Lunch].Should().Be(TimeSpan.FromHours(1.5));
        policy.StandardDurations[ServiceType.Dinner].Should().Be(TimeSpan.FromHours(2));
    }

    #endregion

    #region Computed Properties

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void SlotIntervalMinutes_ReturnsCorrectValue(int minutes)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(minutes), TimeSpan.Zero, 8, 1);

        policy.SlotIntervalMinutes.Should().Be(minutes);
    }

    [Theory]
    [InlineData(7, 7)]
    [InlineData(30, 30)]
    [InlineData(90, 90)]
    public void MaxAdvanceDays_ReturnsCorrectValue(int days, int expectedDays)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(days), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        policy.MaxAdvanceDays.Should().Be(expectedDays);
    }

    #endregion

    #region Business Methods

    [Theory]
    [InlineData(0, 0, true)]   // 00:00
    [InlineData(0, 15, true)]  // 00:15
    [InlineData(0, 30, true)]  // 00:30
    [InlineData(13, 0, true)]  // 13:00
    [InlineData(13, 30, true)] // 13:30
    public void IsValidSlot_WhenTimeIsMultipleOfInterval_ReturnsTrue(int hour, int minute, bool expected)
    {
        // Using 15 minutes interval to ensure 00:15 and 13:30 are valid slots
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(15), TimeSpan.Zero, 8, 1);
        var time = new TimeOnly(hour, minute);

        var result = policy.IsValidSlot(time);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(13, 16, false)] // 13:16 (not multiple of 15)
    [InlineData(13, 44, false)] // 13:44 (not multiple of 15)
    [InlineData(14, 20, false)] // 14:20 (not multiple of 15)
    public void IsValidSlot_WhenTimeIsNotMultipleOfInterval_ReturnsFalse(int hour, int minute, bool expected)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(15), TimeSpan.Zero, 8, 1);
        var time = new TimeOnly(hour, minute);

        var result = policy.IsValidSlot(time);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetDurationFor_WhenDurationExists_ReturnsDuration()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);
        policy.StandardDurationsInternal[ServiceType.Lunch] = TimeSpan.FromHours(1.5);

        var duration = policy.GetDurationFor(ServiceType.Lunch);

        duration.Should().Be(TimeSpan.FromHours(1.5));
    }

    [Fact]
    public void GetDurationFor_WhenDurationNotExists_ReturnsZero()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        var duration = policy.GetDurationFor(ServiceType.Breakfast);

        duration.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(4, true)]
    [InlineData(8, true)]
    public void IsPartySizeValid_WhenWithinRange_ReturnsTrue(int partySize, bool expected)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        var result = policy.IsPartySizeValid(partySize);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(9, false)]
    [InlineData(10, false)]
    public void IsPartySizeValid_WhenOutsideRange_ReturnsFalse(int partySize, bool expected)
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);

        var result = policy.IsPartySizeValid(partySize);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsWithinAdvanceWindow_WhenWithinWindow_ReturnsTrue()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);
        var now = new DateTime(2025, 1, 1, 10, 0, 0);
        var requestedTime = new DateTime(2025, 1, 5, 19, 0, 0); // 4 days in advance

        var result = policy.IsWithinAdvanceWindow(requestedTime, now);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinAdvanceWindow_WhenTooSoon_ReturnsFalse()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);
        var now = new DateTime(2025, 1, 1, 10, 0, 0);
        var requestedTime = new DateTime(2025, 1, 1, 11, 0, 0); // 1 hour in advance (less than 2h minimum)

        var result = policy.IsWithinAdvanceWindow(requestedTime, now);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinAdvanceWindow_WhenTooFar_ReturnsFalse()
    {
        var policy = new TestableReservationPolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(30), TimeSpan.Zero, 8, 1);
        var now = new DateTime(2025, 1, 1, 10, 0, 0);
        var requestedTime = new DateTime(2025, 3, 1, 19, 0, 0); // 59 days in advance (more than 30d maximum)

        var result = policy.IsWithinAdvanceWindow(requestedTime, now);

        result.Should().BeFalse();
    }

    #endregion

    #region Static Instances

    [Fact]
    public void Default_ReturnsCorrectReservationPolicy()
    {
        var instance = ReservationPolicy.Default();

        instance.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(2));
        instance.MaximumAdvanceTime.Should().Be(TimeSpan.FromDays(30));
        instance.SlotInterval.Should().Be(TimeSpan.FromMinutes(30));
        instance.BufferBetweenReservations.Should().Be(TimeSpan.FromMinutes(15));
        instance.MaxPartySize.Should().Be(8);
        instance.MinPartySize.Should().Be(1);
    }

    #endregion
}
