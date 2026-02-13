namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

public partial record PricingTier
{
    /// <summary>
    /// Command handler for activating a <see cref="PricingTier"/> instance.
    /// </summary>
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<PricingTier> pricingTierValidator
    ) : AbstractTransformCommand<PricingTier>
    {
        public override PricingTier Execute(PricingTier current)
        {
            var updated = current with { IsActive = true };

            return pricingTierValidator.ValidateOrThrow(updated);
        }
    }
}
