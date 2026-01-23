namespace Plan.Features.Plans.Domain.PlanAggregate;

/// <summary>
/// Represents a subscription plan as an aggregate root in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// This class follows the MicroDomain pattern where business logic is organized
/// in separate command classes (partial class). The Plan aggregate manages features
/// and payment provider configurations, along with pricing and billing period.
/// </para>
/// <para>
/// A plan is provider-agnostic and maintains configurations for multiple payment
/// providers (Stripe, Paddle, etc.). Customers always have an active Plan that
/// defines their features, price, and usage limits.
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
    /// Gets the price of the plan.
    /// </summary>
    /// <value>The price as a <see cref="Money"/> value object containing amount and currency.</value>
    public Money Price { get; protected set; } = Money.Zero(Currency.EUR);

    /// <summary>
    /// Gets the billing period for this plan.
    /// </summary>
    /// <value>The billing period (Monthly, Quarterly, Semester, or Yearly).</value>
    public BillingPeriod BillingPeriod { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the plan is currently active.
    /// </summary>
    /// <value><c>true</c> if the plan is available for new subscriptions; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// The internal collection of plan features.
    /// </summary>
    protected HashSet<Feature> _features = [];

    /// <summary>
    /// Gets the read-only collection of features in this plan.
    /// </summary>
    /// <value>A read-only collection of <see cref="Feature"/> instances.</value>
    public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of payment provider configurations.
    /// </summary>
    protected HashSet<PaymentProviderConfig> _providerConfigurations = [];

    /// <summary>
    /// Gets the read-only collection of payment provider configurations for this plan.
    /// </summary>
    /// <value>A read-only collection of <see cref="PaymentProviderConfig"/> instances.</value>
    public IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations => _providerConfigurations.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the plan has at least one active provider configuration.
    /// </summary>
    /// <value><c>true</c> if at least one provider configuration is active; otherwise, <c>false</c>.</value>
    public bool HasActiveProvider => _providerConfigurations.Any(p => p.IsActive);

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
    public const string PriceRequired = "Price is required";
    public const string AtLeastOneFeatureRequired = "Plan must have at least one feature";
    public const string AtLeastOneActiveProviderRequired = "Plan must have at least one active provider configuration";
    public const string DuplicateFeatureCode = "Cannot have duplicate feature codes";
    public const string DuplicateActiveProvider = "Cannot have multiple active configurations for the same provider";
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

        RuleFor(x => x.Price)
            .NotNull()
            .WithMessage(PlanValidationMessages.PriceRequired);

        RuleFor(x => x.BillingPeriod)
            .IsInEnum();
    }
}
