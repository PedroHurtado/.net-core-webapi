namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionSetExternalIdsTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.SetExternalIds _setExternalIds = fixture.Get<Subscription.SetExternalIds>();

    [Fact]
    public void Execute_WithValidCommand_SetsExternalIds()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Active)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(DateTimeOffset.UtcNow)
            .WithCurrentPeriodEnd(DateTimeOffset.UtcNow.AddMonths(1))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var command = new SetExternalIdsCommand("sub_xxx", "cus_xxx", "Stripe");

        var result = _setExternalIds.Execute(subscription, command);

        result.ExternalSubscriptionId.Should().Be("sub_xxx");
        result.ExternalCustomerId.Should().Be("cus_xxx");
        result.Provider.Should().Be("Stripe");
    }
}
