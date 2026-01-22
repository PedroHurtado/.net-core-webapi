namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Enums;

public class BillingPeriodTests
{
    [Theory]
    [InlineData(BillingPeriod.Monthly, "Monthly")]
    [InlineData(BillingPeriod.Quarterly, "Quarterly")]
    [InlineData(BillingPeriod.Semester, "Semester")]
    [InlineData(BillingPeriod.Yearly, "Yearly")]
    public void ToString_ReturnsExpectedStringName(BillingPeriod billingPeriod, string expectedName)
    {
        billingPeriod.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly, 1)]
    [InlineData(BillingPeriod.Quarterly, 2)]
    [InlineData(BillingPeriod.Semester, 3)]
    [InlineData(BillingPeriod.Yearly, 4)]
    public void Value_ReturnsExpectedInteger(BillingPeriod billingPeriod, int expectedValue)
    {
        ((int)billingPeriod).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<BillingPeriod>().Should().HaveCount(4);
    }
}
