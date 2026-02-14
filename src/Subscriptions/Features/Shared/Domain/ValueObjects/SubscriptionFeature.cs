namespace Subscriptions.Features.Shared.Domain.ValueObjects;

/// <summary>
/// Represents a feature snapshot associated with a subscription.
/// </summary>
/// <remarks>
/// <para>Captures the feature definition at the time the subscription was created or synced.</para>
/// <para>Used by both Subscription and BillingHistory aggregates as a shared value object.</para>
/// </remarks>
public partial record SubscriptionFeature(
    string Code,
    FeatureType Type,
    int? Limit
);

public static class SubscriptionFeatureValidationMessages
{
    public const string CodeRequired = "Code is required";
    public const string CodeMaxLength = "Code cannot exceed 50 characters";
    public const string CodeUppercase = "Code must be uppercase";
    public const string CodeNoSpaces = "Code cannot contain spaces";
    public const string LimitRequiredWhenTypeIsLimit = "Limit is required when feature type is Limit";
    public const string LimitMustBeGreaterThanZero = "Limit must be greater than 0";
    public const string LimitNotAllowedForBoolean = "Limit is not allowed for Boolean feature type";
    public const string LimitNotAllowedForUnlimited = "Limit is not allowed for Unlimited feature type";
}

/// <summary>
/// Provides validation rules for the <see cref="SubscriptionFeature"/> value object.
/// </summary>
public class SubscriptionFeatureValidator : AbstractValidator<SubscriptionFeature>
{
    public SubscriptionFeatureValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(SubscriptionFeatureValidationMessages.CodeRequired);

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage(SubscriptionFeatureValidationMessages.CodeMaxLength);

        RuleFor(x => x.Code)
            .Must(code => code == code?.ToUpperInvariant())
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(SubscriptionFeatureValidationMessages.CodeUppercase);

        RuleFor(x => x.Code)
            .Must(code => !code.Contains(' '))
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(SubscriptionFeatureValidationMessages.CodeNoSpaces);

        RuleFor(x => x.Limit)
            .NotNull()
            .When(x => x.Type == FeatureType.Limit)
            .WithMessage(SubscriptionFeatureValidationMessages.LimitRequiredWhenTypeIsLimit);

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .When(x => x.Limit.HasValue)
            .WithMessage(SubscriptionFeatureValidationMessages.LimitMustBeGreaterThanZero);

        RuleFor(x => x.Limit)
            .Null()
            .When(x => x.Type == FeatureType.Boolean)
            .WithMessage(SubscriptionFeatureValidationMessages.LimitNotAllowedForBoolean);

        RuleFor(x => x.Limit)
            .Null()
            .When(x => x.Type == FeatureType.Unlimited)
            .WithMessage(SubscriptionFeatureValidationMessages.LimitNotAllowedForUnlimited);
    }
}