namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Represents the configuration for a payment provider specific to this plan.
/// </summary>
/// <remarks>
/// <para>
/// This value object contains the necessary configuration to integrate with
/// a payment provider, mapping the plan to external product and price IDs.
/// </para>
/// <para>
/// Multiple providers can be configured for the same plan, but only one
/// configuration per provider can be active at a time.
/// </para>
/// </remarks>
public partial record PaymentProviderConfig
{
    /// <summary>
    /// Gets the name of the payment provider.
    /// </summary>
    /// <value>The payment provider name (e.g., "Stripe", "Paddle").</value>
    public string Provider { get; }

    /// <summary>
    /// Gets the external product ID in the payment provider.
    /// </summary>
    /// <value>The product ID as defined in the external payment provider system.</value>
    public string ExternalProductId { get; }

    /// <summary>
    /// Gets the external price ID in the payment provider.
    /// </summary>
    /// <value>The price ID as defined in the external payment provider system.</value>
    public string ExternalPriceId { get; }

    /// <summary>
    /// Gets a value indicating whether this configuration is active.
    /// </summary>
    /// <value><c>true</c> if the configuration is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentProviderConfig"/> record.
    /// </summary>
    /// <param name="provider">The payment provider name.</param>
    /// <param name="externalProductId">The external product ID.</param>
    /// <param name="externalPriceId">The external price ID.</param>
    /// <param name="isActive">Whether the configuration is active.</param>
    protected PaymentProviderConfig(
        string provider,
        string externalProductId,
        string externalPriceId,
        bool isActive = true)
    {
        Provider = provider;
        ExternalProductId = externalProductId;
        ExternalPriceId = externalPriceId;
        IsActive = isActive;
    }
}

public static class PaymentProviderConfigValidationMessages
{
    public const string ProviderRequired = "Provider is required";
    public const string ProviderMaxLength = "Provider cannot exceed 50 characters";
    public const string ExternalProductIdRequired = "External product ID is required";
    public const string ExternalProductIdMaxLength = "External product ID cannot exceed 100 characters";
    public const string ExternalPriceIdRequired = "External price ID is required";
    public const string ExternalPriceIdMaxLength = "External price ID cannot exceed 100 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="PaymentProviderConfig"/> value object.
/// </summary>
/// <remarks>
/// Validates payment provider configuration invariants according to the domain specification.
/// </remarks>
public class PaymentProviderConfigValidator : AbstractValidator<PaymentProviderConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentProviderConfigValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public PaymentProviderConfigValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage(PaymentProviderConfigValidationMessages.ProviderRequired);

        RuleFor(x => x.Provider)
            .MaximumLength(50)
            .WithMessage(PaymentProviderConfigValidationMessages.ProviderMaxLength);

        RuleFor(x => x.ExternalProductId)
            .NotEmpty()
            .WithMessage(PaymentProviderConfigValidationMessages.ExternalProductIdRequired);

        RuleFor(x => x.ExternalProductId)
            .MaximumLength(100)
            .WithMessage(PaymentProviderConfigValidationMessages.ExternalProductIdMaxLength);

        RuleFor(x => x.ExternalPriceId)
            .NotEmpty()
            .WithMessage(PaymentProviderConfigValidationMessages.ExternalPriceIdRequired);

        RuleFor(x => x.ExternalPriceId)
            .MaximumLength(100)
            .WithMessage(PaymentProviderConfigValidationMessages.ExternalPriceIdMaxLength);
    }
}
