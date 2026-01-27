namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Enums;

public class FeatureTypeTests
{
    [Theory]
    [InlineData(FeatureType.Boolean, "Boolean")]
    [InlineData(FeatureType.Limit, "Limit")]
    [InlineData(FeatureType.Unlimited, "Unlimited")]
    public void ToString_ReturnsExpectedStringName(FeatureType featureType, string expectedName)
    {
        featureType.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(FeatureType.Boolean, 1)]
    [InlineData(FeatureType.Limit, 2)]
    [InlineData(FeatureType.Unlimited, 3)]
    public void Value_ReturnsExpectedInteger(FeatureType featureType, int expectedValue)
    {
        ((int)featureType).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<FeatureType>().Should().HaveCount(3);
    }
}
