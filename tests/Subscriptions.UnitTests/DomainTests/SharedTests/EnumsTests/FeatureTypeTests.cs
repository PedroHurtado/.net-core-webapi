namespace Subscriptions.UnitTests.DomainTests.SharedTests.EnumsTests;

public class FeatureTypeTests
{
    [Theory]
    [InlineData(FeatureType.Boolean, "Boolean")]
    [InlineData(FeatureType.Limit, "Limit")]
    [InlineData(FeatureType.Unlimited, "Unlimited")]
    public void ToString_ReturnsExpectedStringName(FeatureType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(FeatureType.Boolean, 1)]
    [InlineData(FeatureType.Limit, 2)]
    [InlineData(FeatureType.Unlimited, 3)]
    public void Value_ReturnsExpectedInteger(FeatureType value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<FeatureType>().Should().HaveCount(3);
    }
}
