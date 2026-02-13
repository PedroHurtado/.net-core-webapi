namespace Plans.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Represents a subscription plan as an aggregate root in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// This class follows the MicroDomain pattern where business logic is organized
/// in separate command classes (partial class). The Plan aggregate manages features
/// and pricing tiers, each with their own payment provider configurations.
/// </para>
/// <para>
/// A plan is provider-agnostic and maintains configurations for multiple payment
/// providers (Stripe, Paddle, etc.) within each pricing tier. Customers always have
/// an active Plan that defines their features, pricing options, and usage limits.
/// </para>
/// </remarks>
public partial class Plan : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the name of the plan.
    /// </summary>
    /// <value>The plan name (e.g., "Básico", "Premium"). Maximum length is 100 characters. Must be unique in the system.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the description of the plan.
    /// </summary>
    /// <value>The plan description. Maximum length is 500 characters.</value>
    public string Description { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the plan is currently active.
    /// </summary>
    /// <value><c>true</c> if the plan is available for new subscriptions; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// The internal collection of plan features.
    /// </summary>
    public HashSet<Feature> _features = [];

    /// <summary>
    /// Gets the read-only collection of features in this plan.
    /// </summary>
    /// <value>A read-only collection of <see cref="Feature"/> instances.</value>
    public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of pricing tiers.
    /// </summary>
    protected HashSet<PricingTier> _pricingTiers = [];

    /// <summary>
    /// Gets the read-only collection of pricing tiers for this plan.
    /// </summary>
    /// <value>A read-only collection of <see cref="PricingTier"/> instances.</value>
    public IReadOnlyCollection<PricingTier> PricingTiers => _pricingTiers.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the plan has at least one active pricing tier with an active provider.
    /// </summary>
    /// <value><c>true</c> if at least one pricing tier is active and has an active provider; otherwise, <c>false</c>.</value>
    public bool HasActivePricingTierWithProvider => _pricingTiers.Any(t => t.IsActive && t.HasActiveProvider);

    /// <summary>
    /// Initializes a new instance of the <see cref="Plan"/> class for ORM purposes.
    /// </summary>
    protected Plan() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Plan"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the plan.</param>
    public Plan(Guid id) : base(id) { }
}

public static class PlanValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 100 characters";
    public const string DescriptionRequired = "Description is required";
    public const string DescriptionMaxLength = "Description cannot exceed 500 characters";
    public const string AtLeastOneFeatureRequired = "Plan must have at least one feature";
    public const string AtLeastOneActivePricingTierRequired = "Plan must have at least one active pricing tier with an active provider configuration";
    public const string DuplicateFeatureCode = "Cannot have duplicate feature codes";
    public const string DuplicateBillingPeriod = "Cannot have duplicate billing periods in pricing tiers";
}

/// <summary>
/// Provides validation rules for the <see cref="Plan"/> aggregate root.
/// </summary>
/// <remarks>
/// This validator ensures that plan properties comply with business rules,
/// including required fields, length constraints, and collection invariants.
/// </remarks>
public class PlanValidator : AbstractValidator<Plan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public PlanValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(PlanValidationMessages.IdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(PlanValidationMessages.NameRequired)
            .MaximumLength(100)
            .WithMessage(PlanValidationMessages.NameMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(PlanValidationMessages.DescriptionRequired)
            .MaximumLength(500)
            .WithMessage(PlanValidationMessages.DescriptionMaxLength);
    }
}
