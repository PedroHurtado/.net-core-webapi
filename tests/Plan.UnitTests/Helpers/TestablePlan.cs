namespace Plans.UnitTests.Helpers;

public class TestablePlan : Plans.Features.Plans.Domain.PlanAggregate.Plan
{
    public TestablePlan(Guid id) : base(id) { }

    public void SetName(string name) => Name = name;
    public void SetDescription(string description) => Description = description;
    public void SetIsActive(bool isActive) => IsActive = isActive;

    public new void AddFeature(Feature feature) => _features.Add(feature);
    public new void AddPricingTier(PricingTier tier) => _pricingTiers.Add(tier);
}
