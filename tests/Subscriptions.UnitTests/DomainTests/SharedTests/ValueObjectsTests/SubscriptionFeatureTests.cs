namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class SubscriptionFeatureTests
{
    #region Code

    [Theory]
    [InlineData("RESERVATIONS_MONTHLY")]
    [InlineData("PRIORITY_SUPPORT")]
    public void Code_SetsCorrectValue(string code)
    {
        var vo = new SubscriptionFeature(code, FeatureType.Boolean, null);

        vo.Code.Should().Be(code);
    }

    #endregion

    #region Type

    [Theory]
    [InlineData(FeatureType.Boolean)]
    [InlineData(FeatureType.Limit)]
    [InlineData(FeatureType.Unlimited)]
    public void Type_SetsCorrectValue(FeatureType type)
    {
        var vo = new SubscriptionFeature("TEST_CODE", type, null);

        vo.Type.Should().Be(type);
    }

    #endregion

    #region Limit

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(null)]
    public void Limit_SetsCorrectValue(int? limit)
    {
        var vo = new SubscriptionFeature("TEST_CODE", FeatureType.Limit, limit);

        vo.Limit.Should().Be(limit);
    }

    #endregion
}
