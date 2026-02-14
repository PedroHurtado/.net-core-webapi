namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionExpireTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.Expire _expire = fixture.Get<Subscription.Expire>();

    [Fact]
    public void Execute_FromCancelled_ReturnsExpiredSubscription()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Cancelled)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(DateTimeOffset.UtcNow)
            .WithCurrentPeriodEnd(DateTimeOffset.UtcNow.AddMonths(1))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var result = _expire.Execute(subscription);

        result.Status.Should().Be(SubscriptionStatus.Expired);
        result.IsUsable.Should().BeFalse();
    }

    [Fact]
    public void Execute_FromPastDue_ReturnsExpiredSubscription()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.PastDue)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(DateTimeOffset.UtcNow)
            .WithCurrentPeriodEnd(DateTimeOffset.UtcNow.AddMonths(1))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var result = _expire.Execute(subscription);

        result.Status.Should().Be(SubscriptionStatus.Expired);
    }

    [Fact]
    public void Execute_WhenActive_ThrowsConflictException()
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

        var act = () => _expire.Execute(subscription);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Subscription cannot be expired from current status*");
    }
}
