namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.EnumsTests;

public class SubscriptionStatusTests
{
    [Theory]
    [InlineData(SubscriptionStatus.Trial, "Trial")]
    [InlineData(SubscriptionStatus.Active, "Active")]
    [InlineData(SubscriptionStatus.PastDue, "PastDue")]
    [InlineData(SubscriptionStatus.Cancelled, "Cancelled")]
    [InlineData(SubscriptionStatus.Expired, "Expired")]
    public void ToString_ReturnsExpectedStringName(SubscriptionStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trial, 1)]
    [InlineData(SubscriptionStatus.Active, 2)]
    [InlineData(SubscriptionStatus.PastDue, 3)]
    [InlineData(SubscriptionStatus.Cancelled, 4)]
    [InlineData(SubscriptionStatus.Expired, 5)]
    public void Value_ReturnsExpectedInteger(SubscriptionStatus value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<SubscriptionStatus>().Should().HaveCount(5);
    }
}
