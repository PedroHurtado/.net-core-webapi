namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

/// <summary>
/// Represents an external application connected to a tenant via API key.
/// </summary>
/// <remarks>
/// <para>Domain commands are implemented as partial class methods in separate files.</para>
/// <para>An external app starts as an invitation (Pending status, no user, no API key) and transitions
/// to Accepted when the developer authenticates via OAuth, generating an API key.</para>
/// </remarks>
public partial class ExternalApp : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier of the tenant this external app belongs to.
    /// </summary>
    /// <value>The <see cref="Guid"/> of the owning tenant. Set during creation.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the user associated with this external app.
    /// </summary>
    /// <value>The user linked to this external app, or <c>null</c> if the invitation has not been accepted yet.</value>
    public User? User { get; protected set; }

    /// <summary>
    /// Gets the name of the external application.
    /// </summary>
    /// <value>The application name, maximum 150 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this external app is active.
    /// </summary>
    /// <value><c>true</c> if the external app is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets the email address the invitation was sent to.
    /// </summary>
    /// <value>The email address of the invitee, maximum 254 characters.</value>
    public string InvitationEmail { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    /// <value>The <see cref="InvitationStatus"/> indicating whether the invitation is pending, accepted, or cancelled.</value>
    public InvitationStatus InvitationStatus { get; protected set; }

    /// <summary>
    /// Gets the BCrypt hash of the API key.
    /// </summary>
    /// <value>The BCrypt hash of the API key, or <c>null</c> if no key has been generated.</value>
    public string? ApiKeyHash { get; protected set; }

    /// <summary>
    /// Gets the BCrypt salt used to hash the API key.
    /// </summary>
    /// <value>The salt, or <c>null</c> if no key has been generated.</value>
    public string? ApiKeySalt { get; protected set; }

    /// <summary>
    /// Gets the prefix of the API key for identification.
    /// </summary>
    /// <value>The first 8 characters of the API key, or <c>null</c> if no key has been generated.</value>
    public string? ApiKeyPrefix { get; protected set; }

    /// <summary>
    /// Gets the expiration date of the API key.
    /// </summary>
    /// <value>The expiration date, or <c>null</c> if the key does not expire.</value>
    public DateTime? ApiKeyExpiresAt { get; protected set; }

    /// <summary>
    /// The internal collection of groups.
    /// </summary>
    protected HashSet<string> _groups = [];

    /// <summary>
    /// Gets the read-only collection of groups.
    /// </summary>
    /// <value>A read-only collection of group identifiers.</value>
    public IReadOnlyCollection<string> Groups => _groups.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of additional scopes.
    /// </summary>
    protected HashSet<string> _additionalScopes = [];

    /// <summary>
    /// Gets the read-only collection of additional scopes.
    /// </summary>
    /// <value>A read-only collection of additional scope identifiers.</value>
    public IReadOnlyCollection<string> AdditionalScopes => _additionalScopes.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of excluded scopes.
    /// </summary>
    protected HashSet<string> _excludedScopes = [];

    /// <summary>
    /// Gets the read-only collection of excluded scopes.
    /// </summary>
    /// <value>A read-only collection of excluded scope identifiers.</value>
    public IReadOnlyCollection<string> ExcludedScopes => _excludedScopes.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected ExternalApp() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public ExternalApp(Guid id) : base(id) { }
}

public static class ExternalAppValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "TenantId is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 150 characters";
    public const string InvitationEmailRequired = "Invitation email is required";
    public const string InvitationEmailMaxLength = "Invitation email cannot exceed 254 characters";
    public const string InvitationEmailInvalidFormat = "Invitation email must be a valid email address";
    public const string InvitationStatusInvalid = "Invitation status must be a valid status";
    public const string ApiKeyPrefixMaxLength = "API key prefix cannot exceed 8 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="ExternalApp"/> aggregate root.
/// </summary>
public class ExternalAppValidator : AbstractValidator<ExternalApp>
{
    public ExternalAppValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ExternalAppValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(ExternalAppValidationMessages.TenantIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ExternalAppValidationMessages.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(150)
            .WithMessage(ExternalAppValidationMessages.NameMaxLength);

        RuleFor(x => x.InvitationEmail)
            .NotEmpty()
            .WithMessage(ExternalAppValidationMessages.InvitationEmailRequired);

        RuleFor(x => x.InvitationEmail)
            .MaximumLength(254)
            .WithMessage(ExternalAppValidationMessages.InvitationEmailMaxLength);

        RuleFor(x => x.InvitationEmail)
            .EmailAddress()
            .WithMessage(ExternalAppValidationMessages.InvitationEmailInvalidFormat);

        RuleFor(x => x.InvitationStatus)
            .IsInEnum()
            .WithMessage(ExternalAppValidationMessages.InvitationStatusInvalid);

        RuleFor(x => x.ApiKeyPrefix)
            .MaximumLength(8)
            .WithMessage(ExternalAppValidationMessages.ApiKeyPrefixMaxLength);
    }
}