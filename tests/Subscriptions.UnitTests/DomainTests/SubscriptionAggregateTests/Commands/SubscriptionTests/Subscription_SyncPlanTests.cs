namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionSyncPlanTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.SyncPlan _syncPlan = fixture.Get<Subscription.SyncPlan>();

    [Fact]
    public void Execute_WithNewPlanData_UpdatesAllSnapshots()
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

        var newPrice = new Money(12.99m, Currency.EUR);
        var newFeatures = new List<SubscriptionFeature>
        {
            new("RESERVATIONS_MONTHLY", FeatureType.Limit, 200),
            new("PRIORITY_SUPPORT", FeatureType.Boolean, null)
        };

        var command = new SyncPlanCommand("Plan Básico Plus", BillingPeriod.Quarterly, newPrice, newFeatures);

        var result = _syncPlan.Execute(subscription, command);

        result.PlanName.Should().Be("Plan Básico Plus");
        result.BillingPeriod.Should().Be(BillingPeriod.Quarterly);
        result.Price.Amount.Should().Be(12.99m);
        result.Features.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithFewerFeatures_ReducesFeatures()
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
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100))
            .WithFeature(new SubscriptionFeature("PRIORITY_SUPPORT", FeatureType.Boolean, null))
            .WithFeature(new SubscriptionFeature("ANALYTICS", FeatureType.Boolean, null));

        var newFeatures = new List<SubscriptionFeature>
        {
            new("RESERVATIONS_MONTHLY", FeatureType.Limit, 100)
        };

        var command = new SyncPlanCommand("Plan Básico", BillingPeriod.Monthly, new Money(9.99m, Currency.EUR), newFeatures);

        var result = _syncPlan.Execute(subscription, command);

        result.Features.Should().HaveCount(1);
    }
}
