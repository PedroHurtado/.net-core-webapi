namespace Subscriptions.UnitTests.DomainTests.SharedTests.CommandsTests.SubscriptionFeatureTests;

public class SubscriptionFeatureCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly SubscriptionFeature.Create _create = fixture.Get<SubscriptionFeature.Create>();

    [Fact]
    public void Execute_WithValidLimitFeature_ReturnsSubscriptionFeature()
    {
        var command = new CreateSubscriptionFeatureCommand("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);

        var result = _create.Execute(command);

        result.Code.Should().Be("RESERVATIONS_MONTHLY");
        result.Type.Should().Be(FeatureType.Limit);
        result.Limit.Should().Be(100);
    }

    [Fact]
    public void Execute_WithValidBooleanFeature_ReturnsSubscriptionFeature()
    {
        var command = new CreateSubscriptionFeatureCommand("PRIORITY_SUPPORT", FeatureType.Boolean, null);

        var result = _create.Execute(command);

        result.Code.Should().Be("PRIORITY_SUPPORT");
        result.Type.Should().Be(FeatureType.Boolean);
        result.Limit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithValidUnlimitedFeature_ReturnsSubscriptionFeature()
    {
        var command = new CreateSubscriptionFeatureCommand("RESERVATIONS_MONTHLY", FeatureType.Unlimited, null);

        var result = _create.Execute(command);

        result.Code.Should().Be("RESERVATIONS_MONTHLY");
        result.Type.Should().Be(FeatureType.Unlimited);
        result.Limit.Should().BeNull();
    }
}
