namespace Plan.UnitTests.Helpers;

public class TestablePlan : Plan.Features.Plans.Domain.PlanAggregate.Plan
{
    public TestablePlan(Guid id) : base(id) { }

    public void SetName(string name) => Name = name;
    public void SetDescription(string description) => Description = description;
    public void SetPrice(Money price) => Price = price;
    public void SetBillingPeriod(BillingPeriod billingPeriod) => BillingPeriod = billingPeriod;
    public void SetIsActive(bool isActive) => IsActive = isActive;
    
    public void AddFeature(Feature feature) => _features.Add(feature);
    public void AddProviderConfiguration(PaymentProviderConfig config) => _providerConfigurations.Add(config);
}
