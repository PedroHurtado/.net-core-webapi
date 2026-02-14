namespace Subscriptions.UnitTests.Helpers;

public class TestableSubscription : Subscription
{
    public TestableSubscription(Guid id) : base(id) { }

    public TestableSubscription WithTenantId(Guid value) { TenantId = value; return this; }
    public TestableSubscription WithPlanId(Guid value) { PlanId = value; return this; }
    public TestableSubscription WithPlanName(string value) { PlanName = value; return this; }
    public TestableSubscription WithStatus(SubscriptionStatus value) { Status = value; return this; }
    public TestableSubscription WithBillingPeriod(BillingPeriod value) { BillingPeriod = value; return this; }
    public TestableSubscription WithPrice(Money value) { Price = value; return this; }
    public TestableSubscription WithCurrentPeriodStart(DateTimeOffset value) { CurrentPeriodStart = value; return this; }
    public TestableSubscription WithCurrentPeriodEnd(DateTimeOffset value) { CurrentPeriodEnd = value; return this; }
    public TestableSubscription WithTrialEndsAt(DateTimeOffset? value) { TrialEndsAt = value; return this; }
    public TestableSubscription WithCancelledAt(DateTimeOffset? value) { CancelledAt = value; return this; }
    public TestableSubscription WithCancellationReason(string? value) { CancellationReason = value; return this; }
    public TestableSubscription WithExternalSubscriptionId(string? value) { ExternalSubscriptionId = value; return this; }
    public TestableSubscription WithExternalCustomerId(string? value) { ExternalCustomerId = value; return this; }
    public TestableSubscription WithProvider(string? value) { Provider = value; return this; }
    public TestableSubscription WithFeature(SubscriptionFeature item) { _features.Add(item); return this; }
}
