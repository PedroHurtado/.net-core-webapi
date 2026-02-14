namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests.Commands.SubscriptionTests;

public class SubscriptionCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Subscription.Create _create = fixture.Get<Subscription.Create>();

    [Fact]
    public void Execute_WithValidTrialCommand_ReturnsSubscription()
    {
        var command = new CreateSubscriptionCommand(
            TenantId: Guid.NewGuid(),
            PlanId: Guid.NewGuid(),
            PlanName: "Plan Básico",
            BillingPeriod: BillingPeriod.Monthly,
            Price: new Money(9.99m, Currency.EUR),
            Features: [new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100)],
            CurrentPeriodStart: DateTimeOffset.UtcNow,
            CurrentPeriodEnd: DateTimeOffset.UtcNow.AddDays(14),
            TrialEndsAt: DateTimeOffset.UtcNow.AddDays(14),
            Status: SubscriptionStatus.Trial);

        var result = _create.Execute(command);

        result.Status.Should().Be(SubscriptionStatus.Trial);
        result.IsInTrial.Should().BeTrue();
        result.IsUsable.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithValidActiveCommand_ReturnsSubscription()
    {
        var command = new CreateSubscriptionCommand(
            TenantId: Guid.NewGuid(),
            PlanId: Guid.NewGuid(),
            PlanName: "Plan Básico",
            BillingPeriod: BillingPeriod.Monthly,
            Price: new Money(9.99m, Currency.EUR),
            Features: [new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100)],
            CurrentPeriodStart: DateTimeOffset.UtcNow,
            CurrentPeriodEnd: DateTimeOffset.UtcNow.AddMonths(1),
            TrialEndsAt: null,
            Status: SubscriptionStatus.Active);

        var result = _create.Execute(command);

        result.Status.Should().Be(SubscriptionStatus.Active);
        result.IsUsable.Should().BeTrue();
    }
}
