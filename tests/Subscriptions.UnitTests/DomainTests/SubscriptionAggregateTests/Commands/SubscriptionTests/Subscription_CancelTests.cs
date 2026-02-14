namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionCancelTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.Cancel _cancel = fixture.Get<Subscription.Cancel>();

    [Fact]
    public void Execute_FromActive_ReturnsCancelledSubscription()
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

        var command = new CancelSubscriptionCommand("Too expensive");

        var result = _cancel.Execute(subscription, command);

        result.Status.Should().Be(SubscriptionStatus.Cancelled);
        result.CancelledAt.Should().NotBeNull();
        result.CancellationReason.Should().Be("Too expensive");
        result.IsUsable.Should().BeTrue();
    }

    [Fact]
    public void Execute_FromTrial_ReturnsCancelledSubscription()
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
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var command = new CancelSubscriptionCommand(null);

        var result = _cancel.Execute(subscription, command);

        result.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public void Execute_WithoutReason_ReturnsCancelledSubscription()
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

        var command = new CancelSubscriptionCommand(null);

        var result = _cancel.Execute(subscription, command);

        result.Status.Should().Be(SubscriptionStatus.Cancelled);
        result.CancellationReason.Should().BeNull();
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

        var command = new CancelSubscriptionCommand("Reason");

        var act = () => _cancel.Execute(subscription, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Subscription cannot be cancelled from current status*");
    }

    [Fact]
    public void Execute_WhenAlreadyCancelled_ThrowsConflictException()
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

        var command = new CancelSubscriptionCommand("Reason");

        var act = () => _cancel.Execute(subscription, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Subscription cannot be cancelled from current status*");
    }
}
