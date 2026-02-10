namespace Auth.Features.Sessions.Domain.SessionAggregate;

/// <summary>
/// Represents an authenticated user session in the Fudie platform.
/// </summary>
/// <remarks>
/// <para>The Session aggregate links the opaque browser cookie to the user's identity.
/// It does not store JWTs — the ephemeral JWT is generated per request from session data.</para>
/// <para>Role permissions are denormalized here to avoid an extra Firestore read per request.
/// When role permissions change, affected sessions are destroyed and recreated on re-authentication.</para>
/// </remarks>
public partial class Session : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier of the user who owns this session.
    /// </summary>
    /// <value>The <see cref="Guid"/> of the User aggregate.</value>
    public Guid UserId { get; protected set; }

    /// <summary>
    /// Gets the identifier of the active tenant for this session.
    /// </summary>
    /// <value>The tenant identifier, or null if no tenant context is set.</value>
    public Guid? TenantId { get; protected set; }

    /// <summary>
    /// Gets the identifier of the role assigned in the active tenant.
    /// </summary>
    /// <value>The role identifier, or null when no tenant context is active.</value>
    public Guid? RoleId { get; protected set; }

    /// <summary>
    /// Gets the denormalized group permissions from the role.
    /// </summary>
    /// <value>An array of group permission strings. Empty when no tenant context is active.</value>
    public string[] Groups { get; protected set; } = [];

    /// <summary>
    /// Gets the denormalized additional scopes from the role.
    /// </summary>
    /// <value>An array of individually added scope strings. Empty when no tenant context is active.</value>
    public string[] AdditionalScopes { get; protected set; } = [];

    /// <summary>
    /// Gets the denormalized excluded scopes from the role.
    /// </summary>
    /// <value>An array of individually excluded scope strings. Empty when no tenant context is active.</value>
    public string[] ExcludedScopes { get; protected set; } = [];

    /// <summary>
    /// Gets a value indicating whether the user is a tenant owner.
    /// </summary>
    /// <value><c>true</c> if the user bypasses permission checks; otherwise, <c>false</c>.</value>
    public bool IsOwner { get; protected set; }

    /// <summary>
    /// Gets the date and time when the session was created.
    /// </summary>
    /// <value>The UTC timestamp of session creation.</value>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the date and time of the last activity on this session.
    /// </summary>
    /// <value>The UTC timestamp of the last valid request.</value>
    public DateTimeOffset LastActivityAt { get; protected set; }

    /// <summary>
    /// Gets the date and time when this session expires.
    /// </summary>
    /// <value>The UTC expiration timestamp, calculated as LastActivityAt + 30 days.</value>
    public DateTimeOffset ExpiresAt { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this session has expired.
    /// </summary>
    /// <value><c>true</c> if the current UTC time is past the expiration; otherwise, <c>false</c>.</value>
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    /// <summary>
    /// Gets a value indicating whether this session has an active tenant context.
    /// </summary>
    /// <value><c>true</c> if TenantId is set; otherwise, <c>false</c>.</value>
    public bool HasTenantContext => TenantId != null;

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected Session() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public Session(Guid id) : base(id) { }
}

public static class SessionValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string UserIdRequired = "UserId is required";
    public const string RoleIdRequiredWhenTenantSet = "RoleId is required when TenantId is set";
    public const string RoleIdMustBeNullWhenNoTenant = "RoleId must be null when TenantId is not set";
    public const string ExpiresAtMustBeAfterCreatedAt = "ExpiresAt must be after CreatedAt";
    public const string ExpiresAtMustBeAfterLastActivity = "ExpiresAt must be after LastActivityAt";
}

/// <summary>
/// Provides validation rules for the <see cref="Session"/> aggregate root.
/// </summary>
public class SessionValidator : AbstractValidator<Session>
{
    public SessionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(SessionValidationMessages.IdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(SessionValidationMessages.UserIdRequired);

        RuleFor(x => x.RoleId)
            .NotNull()
            .When(x => x.TenantId != null)
            .WithMessage(SessionValidationMessages.RoleIdRequiredWhenTenantSet);

        RuleFor(x => x.RoleId)
            .Null()
            .When(x => x.TenantId == null)
            .WithMessage(SessionValidationMessages.RoleIdMustBeNullWhenNoTenant);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.CreatedAt)
            .WithMessage(SessionValidationMessages.ExpiresAtMustBeAfterCreatedAt);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.LastActivityAt)
            .WithMessage(SessionValidationMessages.ExpiresAtMustBeAfterLastActivity);
    }
}