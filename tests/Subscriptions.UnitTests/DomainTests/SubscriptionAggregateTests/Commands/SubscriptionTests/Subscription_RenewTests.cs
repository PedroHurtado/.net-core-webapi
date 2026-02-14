namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionRenewTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.Renew _renew = fixture.Get<Subscription.Renew>();

    [Fact]
    public void Execute_WithActiveSubscription_ReturnsRenewedSubscription()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Active)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
            .WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 28, 23, 59, 59, TimeSpan.Zero))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var newStart = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var newEnd = new DateTimeOffset(2026, 3, 31, 23, 59, 59, TimeSpan.Zero);
        var command = new RenewSubscriptionCommand(newStart, newEnd);

        var result = _renew.Execute(subscription, command);

        result.CurrentPeriodStart.Should().Be(newStart);
        result.CurrentPeriodEnd.Should().Be(newEnd);
    }

    [Fact]
    public void Execute_WhenNotActive_ThrowsConflictException()
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

        var command = new RenewSubscriptionCommand(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1));

        var act = () => _renew.Execute(subscription, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Only active subscriptions can be renewed*");
    }
}
