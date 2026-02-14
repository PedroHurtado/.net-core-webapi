namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionActivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.Activate _activate = fixture.Get<Subscription.Activate>();

    [Fact]
    public void Execute_FromTrial_ReturnsActiveSubscription()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Trial)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(DateTimeOffset.UtcNow)
            .WithCurrentPeriodEnd(DateTimeOffset.UtcNow.AddDays(14))
            .WithTrialEndsAt(DateTimeOffset.UtcNow.AddDays(14))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var command = new ActivateSubscriptionCommand(
            ExternalSubscriptionId: "sub_xxx",
            ExternalCustomerId: "cus_xxx",
            CurrentPeriodStart: DateTimeOffset.UtcNow,
            CurrentPeriodEnd: DateTimeOffset.UtcNow.AddMonths(1));

        var result = _activate.Execute(subscription, command);

        result.Status.Should().Be(SubscriptionStatus.Active);
        result.TrialEndsAt.Should().BeNull();
    }

    [Fact]
    public void Execute_FromPastDue_ReturnsActiveSubscription()
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

        var command = new ActivateSubscriptionCommand(
            ExternalSubscriptionId: null,
            ExternalCustomerId: null,
            CurrentPeriodStart: DateTimeOffset.UtcNow,
            CurrentPeriodEnd: DateTimeOffset.UtcNow.AddMonths(1));

        var result = _activate.Execute(subscription, command);

        result.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
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

        var command = new ActivateSubscriptionCommand(null, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1));

        var act = () => _activate.Execute(subscription, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Subscription cannot be activated from current status*");
    }

    [Fact]
    public void Execute_WhenExpired_ThrowsConflictException()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Expired)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(DateTimeOffset.UtcNow)
            .WithCurrentPeriodEnd(DateTimeOffset.UtcNow.AddMonths(1))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var command = new ActivateSubscriptionCommand(null, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1));

        var act = () => _activate.Execute(subscription, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Subscription cannot be activated from current status*");
    }
}
