namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

public partial record PricingTier
{
    /// <summary>
    /// Command handler for deactivating a <see cref="PricingTier"/> instance.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<PricingTier> pricingTierValidator
    ) : AbstractTransformCommand<PricingTier>
    {
        public override PricingTier Execute(PricingTier current)
        {
            var updated = current with { IsActive = false };

            return pricingTierValidator.ValidateOrThrow(updated);
        }
    }
}
