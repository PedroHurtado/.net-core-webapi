namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Represents a pricing option for a subscription plan with a specific billing period.
/// </summary>
/// <remarks>
/// <para>A PricingTier groups a billing period, price, and payment provider configurations.</para>
/// <para>Each plan can have multiple pricing tiers (e.g., Monthly at 9.99 EUR, Yearly at 89.99 EUR),
/// enabling a single plan to offer different billing intervals without duplicating features.</para>
/// </remarks>
public partial record PricingTier(
    BillingPeriod BillingPeriod,
    Money Price,
    bool IsActive,
    IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations
)
{
    /// <summary>
    /// Gets a value indicating whether this pricing tier has at least one active provider configuration.
    /// </summary>
    /// <value><c>true</c> if at least one provider configuration is active; otherwise, <c>false</c>.</value>
    public bool HasActiveProvider => ProviderConfigurations.Any(p => p.IsActive);
}

public static class PricingTierValidationMessages
{
    public const string PriceRequired = "Price is required";
    public const string BillingPeriodInvalid = "Billing period is invalid";
    public const string ProviderConfigurationsRequired = "Provider configurations collection is required";
}

/// <summary>
/// Provides validation rules for the <see cref="PricingTier"/> value object.
/// </summary>
public class PricingTierValidator : AbstractValidator<PricingTier>
{
    public PricingTierValidator()
    {
        RuleFor(x => x.Price)
            .NotNull()
            .WithMessage(PricingTierValidationMessages.PriceRequired);

        RuleFor(x => x.BillingPeriod)
            .IsInEnum()
            .WithMessage(PricingTierValidationMessages.BillingPeriodInvalid);

        RuleFor(x => x.ProviderConfigurations)
            .NotNull()
            .WithMessage(PricingTierValidationMessages.ProviderConfigurationsRequired);
    }
}
