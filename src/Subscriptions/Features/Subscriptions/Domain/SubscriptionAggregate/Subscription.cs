namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

/// <summary>
/// Represents a tenant's subscription to a plan.
/// </summary>
/// <remarks>
/// <para>Manages the lifecycle of a subscription including trial, activation, renewal, cancellation and expiration.</para>
/// <para>Contains snapshots of plan data (name, price, features) at the time of creation or sync.</para>
/// </remarks>
public partial class Subscription : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the tenant identifier that owns this subscription.
    /// </summary>
    /// <value>The unique identifier of the tenant from the customer-service.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the plan identifier this subscription is based on.
    /// </summary>
    /// <value>The unique identifier of the plan from the plan-service.</value>
    public Guid PlanId { get; protected set; }

    /// <summary>
    /// Gets the snapshot of the plan name at subscription time.
    /// </summary>
    /// <value>The plan name, maximum 100 characters.</value>
    public string PlanName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the current lifecycle status of the subscription.
    /// </summary>
    /// <value>The <see cref="SubscriptionStatus"/> indicating the current state.</value>
    public SubscriptionStatus Status { get; protected set; }

    /// <summary>
    /// Gets the billing frequency snapshot.
    /// </summary>
    /// <value>The <see cref="Enums.BillingPeriod"/> from the selected pricing tier.</value>
    public BillingPeriod BillingPeriod { get; protected set; }

    /// <summary>
    /// Gets the price snapshot including currency.
    /// </summary>
    /// <value>The <see cref="Money"/> amount from the selected pricing tier.</value>
    public Money Price { get; protected set; } = default!;

    /// <summary>
    /// Gets the start date of the current billing period.
    /// </summary>
    /// <value>The UTC date when the current period started.</value>
    public DateTimeOffset CurrentPeriodStart { get; protected set; }

    /// <summary>
    /// Gets the end date of the current billing period.
    /// </summary>
    /// <value>The UTC date when the current period ends.</value>
    public DateTimeOffset CurrentPeriodEnd { get; protected set; }

    /// <summary>
    /// Gets the date when the trial period ends.
    /// </summary>
    /// <value>The UTC date of trial expiration, or null if not in trial.</value>
    public DateTimeOffset? TrialEndsAt { get; protected set; }

    /// <summary>
    /// Gets the date when the subscription was cancelled.
    /// </summary>
    /// <value>The UTC date of cancellation, or null if not cancelled.</value>
    public DateTimeOffset? CancelledAt { get; protected set; }

    /// <summary>
    /// Gets the reason provided for cancellation.
    /// </summary>
    /// <value>The cancellation reason text, maximum 500 characters, or null.</value>
    public string? CancellationReason { get; protected set; }

    /// <summary>
    /// Gets the external subscription identifier from the payment provider.
    /// </summary>
    /// <value>The Stripe subscription ID (sub_xxx), maximum 100 characters, or null.</value>
    public string? ExternalSubscriptionId { get; protected set; }

    /// <summary>
    /// Gets the external customer identifier from the payment provider.
    /// </summary>
    /// <value>The Stripe customer ID (cus_xxx), maximum 100 characters, or null.</value>
    public string? ExternalCustomerId { get; protected set; }

    /// <summary>
    /// Gets the payment provider name.
    /// </summary>
    /// <value>The provider name (e.g. "Stripe"), maximum 50 characters, or null.</value>
    public string? Provider { get; protected set; }

    /// <summary>
    /// The internal collection of subscription features.
    /// </summary>
    protected HashSet<SubscriptionFeature> _features = [];

    /// <summary>
    /// Gets the read-only collection of subscription features.
    /// </summary>
    /// <value>A read-only collection of <see cref="SubscriptionFeature"/> instances.</value>
    public IReadOnlyCollection<SubscriptionFeature> Features => _features.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the subscription is currently in a trial period.
    /// </summary>
    /// <value><c>true</c> if status is Trial and trial end date is in the future; otherwise, <c>false</c>.</value>
    public bool IsInTrial => Status == SubscriptionStatus.Trial && TrialEndsAt > DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the subscription is usable by the tenant.
    /// </summary>
    /// <value><c>true</c> if status is Trial, Active, PastDue or Cancelled; otherwise, <c>false</c>.</value>
    public bool IsUsable => Status == SubscriptionStatus.Trial
        || Status == SubscriptionStatus.Active
        || Status == SubscriptionStatus.PastDue
        || Status == SubscriptionStatus.Cancelled;

    /// <summary>
    /// Gets a value indicating whether the subscription has an external payment provider.
    /// </summary>
    /// <value><c>true</c> if ExternalSubscriptionId is not null or empty; otherwise, <c>false</c>.</value>
    public bool HasExternalProvider => !string.IsNullOrEmpty(ExternalSubscriptionId);

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected Subscription() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public Subscription(Guid id) : base(id) { }
}

public static class SubscriptionValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "TenantId is required";
    public const string PlanIdRequired = "PlanId is required";
    public const string PlanNameRequired = "PlanName is required";
    public const string PlanNameMaxLength = "PlanName cannot exceed 100 characters";
    public const string PriceRequired = "Price is required";
    public const string CurrentPeriodStartRequired = "CurrentPeriodStart is required";
    public const string CurrentPeriodEndRequired = "CurrentPeriodEnd is required";
    public const string CurrentPeriodStartMustBeEarlier = "CurrentPeriodStart must be earlier than CurrentPeriodEnd";
    public const string CancellationReasonMaxLength = "CancellationReason cannot exceed 500 characters";
    public const string ExternalSubscriptionIdMaxLength = "ExternalSubscriptionId cannot exceed 100 characters";
    public const string ExternalCustomerIdMaxLength = "ExternalCustomerId cannot exceed 100 characters";
    public const string ProviderMaxLength = "Provider cannot exceed 50 characters";
    public const string FeaturesRequired = "Features is required";
}

/// <summary>
/// Provides validation rules for the <see cref="Subscription"/> aggregate root.
/// </summary>
public class SubscriptionValidator : AbstractValidator<Subscription>
{
    public SubscriptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.TenantIdRequired);

        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.PlanIdRequired);

        RuleFor(x => x.PlanName)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.PlanNameRequired);

        RuleFor(x => x.PlanName)
            .MaximumLength(100)
            .WithMessage(SubscriptionValidationMessages.PlanNameMaxLength);

        RuleFor(x => x.Price)
            .NotNull()
            .WithMessage(SubscriptionValidationMessages.PriceRequired);

        RuleFor(x => x.CurrentPeriodStart)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.CurrentPeriodStartRequired);

        RuleFor(x => x.CurrentPeriodEnd)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.CurrentPeriodEndRequired);

        RuleFor(x => x.CurrentPeriodStart)
            .LessThan(x => x.CurrentPeriodEnd)
            .WithMessage(SubscriptionValidationMessages.CurrentPeriodStartMustBeEarlier);

        RuleFor(x => x.CancellationReason)
            .MaximumLength(500)
            .WithMessage(SubscriptionValidationMessages.CancellationReasonMaxLength);

        RuleFor(x => x.ExternalSubscriptionId)
            .MaximumLength(100)
            .WithMessage(SubscriptionValidationMessages.ExternalSubscriptionIdMaxLength);

        RuleFor(x => x.ExternalCustomerId)
            .MaximumLength(100)
            .WithMessage(SubscriptionValidationMessages.ExternalCustomerIdMaxLength);

        RuleFor(x => x.Provider)
            .MaximumLength(50)
            .WithMessage(SubscriptionValidationMessages.ProviderMaxLength);

        RuleFor(x => x.Features)
            .NotEmpty()
            .WithMessage(SubscriptionValidationMessages.FeaturesRequired);
    }
}
