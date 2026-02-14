namespace Subscriptions.UnitTests.DomainTests.SharedTests.EnumsTests;

public class BillingPeriodTests
{
    [Theory]
    [InlineData(BillingPeriod.Monthly, "Monthly")]
    [InlineData(BillingPeriod.Quarterly, "Quarterly")]
    [InlineData(BillingPeriod.Semester, "Semester")]
    [InlineData(BillingPeriod.Yearly, "Yearly")]
    public void ToString_ReturnsExpectedStringName(BillingPeriod value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly, 1)]
    [InlineData(BillingPeriod.Quarterly, 2)]
    [InlineData(BillingPeriod.Semester, 3)]
    [InlineData(BillingPeriod.Yearly, 4)]
    public void Value_ReturnsExpectedInteger(BillingPeriod value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<BillingPeriod>().Should().HaveCount(4);
    }
}
